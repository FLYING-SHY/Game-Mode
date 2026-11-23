using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace CoH3MemoryTool;

internal static class MatchHudUiFixture
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Type Suite = typeof(SessionResetFixture);
    private static readonly Type FormType = Suite.Assembly.GetType("CoH3MemoryTool.OverlaySettingsForm")
        ?? Suite.Assembly.GetType("CoH3MemoryTool.ControlBarForm", true);
    private static readonly List<IDisposable> Owned = (List<IDisposable>)Suite.GetField("Owned", Members).GetValue(null);
    private static int assertions;

    private static object Shared(string name, params object[] args) => Suite.GetMethod(name, Members).Invoke(null, args);
    private static object Get(object target, string name) => target.GetType().GetField(name, Members).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, Members).SetValue(target, value);
    private static void Invoke(object target, string name, params object[] args) => FormType.GetMethod(name, Members).Invoke(target, args);
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        assertions++;
        Console.WriteLine("PASS " + label);
    }

    private static ScenePublication<MarkerScene> Scene(object form, string host) =>
        (ScenePublication<MarkerScene>)Get(Get(Get(form, host), "window"), "scene");

    private static void SuspendCases(object form)
    {
        var polling = new Timer();
        var status = new Label();
        Owned.Add(polling);
        Owned.Add(status);
        Set(form, "polling", polling);
        Set(form, "status", status);
        object reader = Shared("ReaderIdentity"); // Managed identity only; owns no game process/handle.
        Set(reader, "miniEpoch", 73L);
        Set(form, "reader", reader);
        DateTime retry = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        Set(form, "retryAt", retry);
        foreach (MatchHudOutcome outcome in new[] { MatchHudOutcome.Resolving, MatchHudOutcome.Retired, MatchHudOutcome.ReadFault, MatchHudOutcome.Ambiguous })
        {
            Shared("Seed", form, "old-product-key");
            polling.Interval = 1000;
            var air = Scene(form, "airOverlay");
            var squad = Scene(form, "squadOverlay");
            var marker = (MarkerScene)FormatterServices.GetUninitializedObject(typeof(MarkerScene));
            air.Publish(marker);
            squad.Publish(marker);
            air.Capture(out long airToken);
            squad.Capture(out long squadToken);
            var error = new MatchHudUnavailableException(outcome, "fixture: " + outcome);
            Invoke(form, "SuspendMatchContext", error);
            Shared("CheckReset", form, "suspend " + outcome);
            Check(ReferenceEquals(Get(form, "reader"), reader) && (int)Get(form, "pid") == 42,
                outcome + " retains reader identity and PID");
            Check((long)Get(reader, "miniEpoch") == 73L && !(bool)Get(reader, "miniDisposed"),
                outcome + " preserves in-progress discovery identity");
            Check(polling.Interval == 250 && !polling.Enabled && (DateTime)Get(form, "retryAt") == retry,
                outcome + " schedules normal 250ms poll without starting timer or reconnect backoff");
            var recovery = (ReadRecovery55)Get(form, "recovery55");
            Check((int)Get(recovery, "failures") == 0 && (DateTime)Get(recovery, "first") == DateTime.MinValue,
                outcome + " clears fast-retry history");
            Check(status.Text == error.Message, outcome + " surfaces actual diagnostic");
            bool painted = false;
            Check(air.Capture(out long nextAir) == null && nextAir > airToken
                && squad.Capture(out long nextSquad) == null && nextSquad > squadToken,
                outcome + " revokes real air and squad publications");
            Check(!air.TryPresent(airToken, () => painted = true)
                && !squad.TryPresent(squadToken, () => painted = true) && !painted,
                outcome + " blocks late frame from revoked publication");
        }
        var fresh = new List<PlayerInfoRow> { new PlayerInfoRow { Relation = "敌方", Slot = 1, PlayerIdentity = 1001 } };
        Invoke(form, "AcceptSnapshot", fresh, "new-product-key");
        Check(ReferenceEquals(Get(form, "reader"), reader) && ReferenceEquals(Get(form, "rows"), fresh)
            && (string)Get(form, "rowsMatch") == "new-product-key", "fresh qualified key resumes using retained reader");
        Set(form, "reader", null);
    }

    private static void LayerPropagation(object form)
    {
        Shared("Seed", form, "layer-key");
        var error = new MatchHudUnavailableException(MatchHudOutcome.Retired, "retired during layer");
        bool escaped = false;
        try { Invoke(form, "Layer55", "fixture", (Action)(() => { throw error; })); }
        catch (TargetInvocationException ex) { escaped = ReferenceEquals(ex.InnerException, error); }
        Check(escaped, "Layer55 propagates the exact typed signal to Poll");
        Check((bool)Get(form, "liveRows"), "Layer55 delegates session cleanup to the owning Poll handler");
        Invoke(form, "Layer55", "fixture ordinary fault", (Action)(() => { throw new InvalidOperationException("ordinary data fault"); }));
        Check((bool)Get(form, "liveRows") && ((Label)Get(form, "status")).Text.Contains("ordinary data fault"),
            "ordinary layer error keeps existing local diagnostic behavior");
        Invoke(form, "ClearSessionData");
    }

    private static IEnumerable<(int Offset, OpCode Code, MethodBase Call)> Instructions(MethodInfo method)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null))
            .ToDictionary(c => unchecked((ushort)c.Value));
        byte[] il = method.GetMethodBody().GetILAsByteArray();
        for (int offset = 0; offset < il.Length;)
        {
            int start = offset;
            ushort value = il[offset++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
            OpCode code = codes[value];
            MethodBase call = code.OperandType == OperandType.InlineMethod
                ? method.Module.ResolveMethod(BitConverter.ToInt32(il, offset)) : null;
            yield return (start, code, call);
            switch (code.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineBrTarget: case OperandType.ShortInlineI: case OperandType.ShortInlineVar: offset++; break;
                case OperandType.InlineVar: offset += 2; break;
                case OperandType.InlineI8: case OperandType.InlineR: offset += 8; break;
                case OperandType.InlineSwitch: offset += 4 + 4 * BitConverter.ToInt32(il, offset); break;
                default: offset += 4; break;
            }
        }
    }

    private static void CheckTypedHandler(string name, bool rethrow)
    {
        var method = FormType.GetMethod(name, Members);
        var typed = method.GetMethodBody().ExceptionHandlingClauses
            .Where(c => c.Flags == ExceptionHandlingClauseOptions.Clause && c.CatchType == typeof(MatchHudUnavailableException)).ToArray();
        Check(typed.Length > 0, name + " has actual compiled typed handler");
        foreach (var clause in typed)
        {
            var region = Instructions(method).Where(i => i.Offset >= clause.HandlerOffset && i.Offset < clause.HandlerOffset + clause.HandlerLength).ToArray();
            bool forwards = region.Any(i => i.Code == OpCodes.Rethrow);
            bool suspends = region.Any(i => i.Call?.DeclaringType == FormType && i.Call.Name == "SuspendMatchContext");
            Check(rethrow ? forwards : suspends || (name == "Poll" && forwards),
                name + " compiled handler forwards or suspends through shared cleanup");
            Check(!region.Any(i => i.Call?.Name == "Retry" || i.Call?.Name == "DropReader" || i.Call?.Name == "Dispose"),
                name + " typed handler avoids fast retry and reader disposal");
        }
        if (name == "Poll")
        {
            Check(method.GetMethodBody().ExceptionHandlingClauses.Any(c => c.Flags == ExceptionHandlingClauseOptions.Clause
                && c.CatchType == typeof(Exception)), "Poll retains generic data-fault handler");
            int qualification = Instructions(method).Single(i => i.Call?.Name == "IsCurrentMatch55").Offset;
            Check(typed.Any(c => qualification >= c.TryOffset && qualification < c.TryOffset + c.TryLength
                && Instructions(method).Any(i => i.Offset >= c.HandlerOffset && i.Offset < c.HandlerOffset + c.HandlerLength
                    && i.Call?.Name == "SuspendMatchContext")), "qualification inside ordinary catch has its own typed suspension handler");
        }
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            Console.WriteLine("Fixture=" + FormType.Name + "; level=F; linked production forms, reset and publication; no constructors/tracking/license changes/native game");
            Check(!LicenseGate60.Allowed, "normal license gate remains inactive");
            object form = Shared("NewForm");
            SuspendCases(form);
            LayerPropagation(form);
            CheckTypedHandler("Poll", false);
            CheckTypedHandler("PollSupport49", false);
            CheckTypedHandler("Layer55", true);
            if (FormType.GetMethod("TickFlank47", Members) != null) CheckTypedHandler("TickFlank47", false);
            var controls = Owned.OfType<Control>()
                .Concat(Owned.OfType<SquadMarkerHost>().Select(h => (Control)Get(h, "window")))
                .Concat(Owned.OfType<AirOverlayHost>().Select(h => (Control)Get(h, "window")));
            Check(controls.All(c => !c.IsHandleCreated), "created no window handles");
            Check(Owned.OfType<SquadMarkerHost>().All(h => Get(h, "thread") == null)
                && Owned.OfType<AirOverlayHost>().All(h => Get(h, "thread") == null), "started no renderer threads");
            Check(!LicenseGate60.Allowed, "license gate unchanged after all fixture cases");
            Console.WriteLine("RESULT PASS assertions=" + assertions + "; productGUI=not_run; realGame=not_run");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.ToString()); return 1; }
        finally { for (int i = Owned.Count - 1; i >= 0; i--) Owned[i].Dispose(); }
    }
}
