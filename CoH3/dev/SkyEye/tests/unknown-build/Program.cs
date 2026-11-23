using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class UnknownBuildFixture
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const string LayoutStop = "游戏版本未登记布局，结构依赖功能已暂停。请等待版本适配。";
    private static readonly Type ReaderType = typeof(PlayerInfoReader);
    private static readonly Dictionary<short, OpCode> Opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null))
        .ToDictionary(opcode => opcode.Value);
    private static readonly List<Dictionary<string, object>> Results = new List<Dictionary<string, object>>();

    private static int Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Expected one result JSON path.");
        foreach (GameVersion45.GameBuild build in new[]
        {
            GameVersion45.GameBuild.Build2406,
            GameVersion45.GameBuild.Build52426,
            GameVersion45.GameBuild.Unknown,
            (GameVersion45.GameBuild)123456
        })
        {
            foreach (bool maintenance in new[] { false, true })
                RunScenario(build, maintenance);
        }

        foreach (ConstructorInfo constructor in ReaderType.GetConstructors(PrivateInstance)
            .Where(candidate => candidate.GetParameters().LastOrDefault()?.ParameterType == typeof(bool)))
        {
            Check("constructor IL order " + constructor, () => CheckConstructorOrder(constructor));
        }

        int passed = Results.Count(result => (bool)result["passed"]);
        int failed = Results.Count - passed;
        var summary = new Dictionary<string, object>
        {
            ["level"] = "F",
            ["assembly"] = ReaderType.Assembly.GetName().Name,
#if MAINTENANCE_DIAGNOSTICS
            ["configuration"] = "Maintenance",
#else
            ["configuration"] = "Release",
#endif
            ["scenarios"] = 8,
            ["passed"] = passed,
            ["failed"] = failed,
            ["reader_constructors_executed"] = 0,
            ["native_handles_created"] = 0,
            ["game_validation"] = "not_run",
            ["cases"] = Results
        };
        File.WriteAllText(args[0], new JavaScriptSerializer().Serialize(summary));
        Console.WriteLine("RESULT passed=" + passed + " failed=" + failed + " scenarios=8; level=F; constructors=0; native_handles=0");
        return failed == 0 ? 0 : 1;
    }

    private static void RunScenario(GameVersion45.GameBuild build, bool maintenance)
    {
        // Allocate only managed storage. No constructor, field initializer, process lookup or P/Invoke runs.
        object reader = FormatterServices.GetUninitializedObject(ReaderType);
        ReaderType.GetField("detectedBuild", PrivateInstance).SetValue(reader, build);
        ReaderType.GetField("maintenanceDiagnostics", PrivateInstance).SetValue(reader, maintenance);
        bool known = build == GameVersion45.GameBuild.Build2406 || build == GameVersion45.GameBuild.Build52426;
        bool candidate = build == GameVersion45.GameBuild.Build52426;
        string prefix = "build=" + build + " maintenance=" + maintenance + " ";

        Check(prefix + "UsesCandidateLayout", () =>
        {
            if (!known) ExpectLayoutStop(() => Invoke(reader, "UsesCandidateLayout"));
            else Equal(candidate, Invoke(reader, "UsesCandidateLayout"));
        });
        Check(prefix + "ValidateLayout", () =>
        {
            if (!known) ExpectLayoutStop(() => Invoke(reader, "ValidateLayout"));
            else Invoke(reader, "ValidateLayout");
        });
        Check(prefix + "AirMoverShape45", () =>
        {
            if (!known) ExpectLayoutStop(() => Invoke(reader, "AirMoverShape45"));
            else Equal(candidate ? AirMoverShape.Candidate52426_184 : AirMoverShape.Legacy284, Invoke(reader, "AirMoverShape45"));
        });
        Check(prefix + "BuildRva45 all registered roles", () =>
        {
            if (!known) ExpectLayoutStop(() => Invoke(reader, "BuildRva45", 112918560L));
            else
            {
                // Exercise production dispatch for every role; the layout bytes are separately SHA-bound.
                var roles = (Dictionary<long, long>)typeof(GameLayout52426)
                    .GetField("candidates", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                foreach (KeyValuePair<long, long> role in roles)
                    Equal(candidate ? role.Value : role.Key, Invoke(reader, "BuildRva45", role.Key));
            }
        });
        Check(prefix + "LegacyTransformRva45 all five transform roles", () =>
        {
            if (!known) ExpectLayoutStop(() => Invoke(reader, "LegacyTransformRva45", 0x3D73280L));
            else
            {
                long[] legacy = { 64430720L, 64431008L, 64432864L, 64815280L, 64815656L };
                long[] actual = { 0x3D73280L, 0x3D733A0L, 0x3D73AE0L, 0x3DD2D00L, 0x3DD2E78L };
                for (int index = 0; index < legacy.Length; index++)
                    Equal(legacy[index], Invoke(reader, "LegacyTransformRva45", candidate ? actual[index] : legacy[index]));
            }
        });
        Check(prefix + "no process handle or field initialization", () =>
        {
            foreach (string field in new[] { "process", "handle", "resources", "airTracker" })
                Equal(null, ReaderType.GetField(field, PrivateInstance).GetValue(reader));
        });
    }

    private static object Invoke(object reader, string method, params object[] args)
    {
        try
        {
            return ReaderType.GetMethod(method, PrivateInstance).Invoke(reader, args);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException;
        }
    }

    private static void ExpectLayoutStop(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException exception)
        {
            Equal(LayoutStop, exception.Message);
            return;
        }
        throw new Exception("Expected InvalidOperationException from unregistered build layout.");
    }

    private static void Equal(object expected, object actual)
    {
        if (!Equals(expected, actual))
            throw new Exception("Expected " + (expected ?? "<null>") + ", got " + (actual ?? "<null>"));
    }

    private static void Check(string name, Action action)
    {
        try
        {
            action();
            Results.Add(new Dictionary<string, object> { ["name"] = name, ["passed"] = true });
            Console.WriteLine("PASS " + name);
        }
        catch (Exception exception)
        {
            Results.Add(new Dictionary<string, object>
            {
                ["name"] = name,
                ["passed"] = false,
                ["error"] = exception.GetType().Name + ": " + exception.Message
            });
            Console.WriteLine("FAIL " + name + ": " + exception);
        }
    }

    private static void CheckConstructorOrder(ConstructorInfo constructor)
    {
        var calls = Calls(constructor).ToList();
        int fingerprint = calls.FindIndex(call => call.Item2.DeclaringType == typeof(GameVersion45) && call.Item2.Name == "RequireVerified");
        int validate = calls.FindIndex(call => call.Item2.DeclaringType == ReaderType && call.Item2.Name == "ValidateLayout");
        int license = calls.FindIndex(call => call.Item2.DeclaringType == typeof(LicenseGate60) && call.Item2.Name == "Matches");
        int open = calls.FindIndex(call => call.Item2.DeclaringType == typeof(Native) && call.Item2.Name == "OpenProcess");
        if (!(fingerprint >= 0 && fingerprint < validate && validate < license && license < open))
            throw new Exception("Expected RequireVerified < ValidateLayout < LicenseGate60.Matches < Native.OpenProcess.");
        Console.WriteLine("IL " + constructor + " RequireVerified@" + calls[fingerprint].Item1
            + " ValidateLayout@" + calls[validate].Item1 + " LicenseGate@" + calls[license].Item1 + " OpenProcess@" + calls[open].Item1);
    }

    private static IEnumerable<Tuple<int, MethodBase>> Calls(MethodBase method)
    {
        byte[] il = method.GetMethodBody().GetILAsByteArray();
        int offset = 0;
        while (offset < il.Length)
        {
            int instruction = offset;
            short value = il[offset++];
            if (value == 0xFE) value = unchecked((short)(0xFE00 | il[offset++]));
            OpCode opcode = Opcodes[value];
            if (opcode.OperandType == OperandType.InlineMethod)
            {
                int token = BitConverter.ToInt32(il, offset);
                if (opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                    yield return Tuple.Create(instruction, method.Module.ResolveMethod(token));
            }
            switch (opcode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    offset += 1;
                    break;
                case OperandType.InlineVar:
                    offset += 2;
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    offset += 4;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    offset += 8;
                    break;
                case OperandType.InlineSwitch:
                    offset += 4 + 4 * BitConverter.ToInt32(il, offset);
                    break;
                default:
                    throw new Exception("Unexpected IL operand: " + opcode.OperandType);
            }
        }
    }
}
