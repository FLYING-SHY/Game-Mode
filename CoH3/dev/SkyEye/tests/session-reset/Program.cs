using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class SessionResetFixture
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Type FormType = typeof(SessionResetFixture).Assembly.GetType("CoH3MemoryTool.OverlaySettingsForm")
        ?? typeof(SessionResetFixture).Assembly.GetType("CoH3MemoryTool.ControlBarForm", true);
    private static readonly List<IDisposable> Owned = new List<IDisposable>();
    private static int assertions;

    private static object Get(object target, string field) => target.GetType().GetField(field, Fields).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, Fields).SetValue(target, value);
    private static bool Has(string field) => FormType.GetField(field, Fields) != null;
    private static void Invoke(object target, string method, params object[] arguments) => target.GetType().GetMethod(method, Fields).Invoke(target, arguments);
    private static T Own<T>(T value) where T : IDisposable { Owned.Add(value); return value; }
    private static void Check(bool result, string name)
    {
        if (!result) throw new Exception(name);
        assertions++;
        Console.WriteLine("PASS " + name);
    }

    // Skip both form constructors: they include discovery/tray/settings behaviour unrelated to reset.
    // Only real production dependencies used by the reset are created; all overlays use track:false.
    private static object NewForm()
    {
        object form = FormatterServices.GetUninitializedObject(FormType);
        OverlayPreferences prefs = new OverlayPreferences { Enabled = false, FontSize = 23, ShowMines = false };
        Set(form, "preferences", prefs);
        Set(form, "selection", new EnemySelection());
        Set(form, "armourSelection", new EnemySelection());
        Set(form, "alerts", new AlertTracker());
        Set(form, "armourAlerts", new ArmourProductionTracker());
        Set(form, "recovery55", new ReadRecovery55());
        Set(form, "supportSeen49", new HashSet<string>());
        Set(form, "rows", new List<PlayerInfoRow>());
        Set(form, "bindings46", Activator.CreateInstance(FormType.GetField("bindings46", Fields).FieldType));
        Set(form, "overlay", Own(new EnemyOverlayWindow(prefs, track: false)));
        Set(form, "mineOverlay", Own(new MineOverlayWindow(prefs, track: false)));
        Set(form, "miniOverlay", Own(new MinimapMarkerWindow(prefs.Minimap, track: false, prefs.Tactical51)));
        Set(form, "squadOverlay", Own(new SquadMarkerHost(prefs.SquadMarkers, track: false)));
        Set(form, "airOverlay", Own(new AirOverlayHost(prefs.SquadMarkers, track: false, prefs.Tactical51)));
        if (Has("supportActive49"))
        {
            Set(form, "supportActive49", new Dictionary<string, SupportCommand49>());
            Set(form, "flankUnits47", new List<SquadMarker>());
            Set(form, "enemies", Own(new CheckedListBox()));
            Set(form, "preview", Own(new Panel()));
            Set(form, "armourList46", Own(new CheckedListBox()));
            Set(form, "flankEnabled47", true);
            Set(form, "flankRange52", false);
            Set(form, "flankRadius47", 77);
            Set(form, "flankKey47", "F11");
        }
        return form;
    }

    private static PlayerInfoRow Enemy() => new PlayerInfoRow
    {
        Slot = 2, PlayerIdentity = 19, Name = "same-enemy", Relation = "敌方", ProductionKnown = true
    };

    private static ProductionJob Job() => new ProductionJob("sbps\\races\\german\\vehicles\\panzer_iv.rgd", 20, 100, "reused-job");

    private static object ReaderIdentity()
    {
        object reader = FormatterServices.GetUninitializedObject(typeof(PlayerInfoReader));
        Set(reader, "miniGate", new object());
        return reader; // No process or handle; Dispose only invalidates this managed scan epoch.
    }

    private static PlayerInfoRow Seed(object form, string match)
    {
        PlayerInfoRow enemy = Enemy();
        enemy.Producing.Add(Job());
        enemy.Buildings.Add(new TrackedBuilding { Key = "old-building", Complete = true, Tier = 2 });
        var rows = new List<PlayerInfoRow> { enemy };
        var selection = (EnemySelection)Get(form, "selection");
        var armourSelection = (EnemySelection)Get(form, "armourSelection");
        selection.UpdateMatch(match);
        armourSelection.UpdateMatch(match);
        ((AlertTracker)Get(form, "alerts")).Update(match, rows, selection);
        ((ArmourProductionTracker)Get(form, "armourAlerts")).Update(match, rows, armourSelection);
        selection.Set(enemy, false);
        armourSelection.Set(enemy, false);
        Set(form, "rows", rows);
        Set(form, "rowsMatch", match);
        Set(form, "liveRows", true);
        Set(form, "pid", 42);
        Set(form, "supportMatch49", match);
        Set(form, "supportBaseline49", true);
        ((HashSet<string>)Get(form, "supportSeen49")).Add("reused-support");
        ((ReadRecovery55)Get(form, "recovery55")).Retry(true, DateTime.UtcNow);
        if (Has("supportActive49"))
        {
            ((Dictionary<string, SupportCommand49>)Get(form, "supportActive49")).Add("old", new SupportCommand49());
            ((List<SquadMarker>)Get(form, "flankUnits47")).Add(new SquadMarker());
            Set(form, "flankMatch47", match);
            Set(form, "flankActive47", true);
            Set(form, "flankWasDown47", true);
            Set(form, "flankArmed47", DateTime.UtcNow);
            Set(form, "flankFresh47", DateTime.UtcNow);
            Set(form, "flankNextSound47", DateTime.UtcNow);
            Set(form, "flankX47", 1f);
            Set(form, "flankY47", 2f);
            Set(form, "flankZ47", 3f);
            Set(form, "flankSource47", 7L);
            Set(form, "flankIdentity47", 8L);
        }
        return enemy;
    }

    private static void CheckReset(object form, string label)
    {
        Check(!(bool)Get(form, "liveRows") && Get(form, "rowsMatch") == null, label + " rows identity invalidated");
        Check(((List<PlayerInfoRow>)Get(form, "rows")).Count == 0, label + " old rows removed");
        Check(Get(form, "supportMatch49") == null && !(bool)Get(form, "supportBaseline49")
            && ((HashSet<string>)Get(form, "supportSeen49")).Count == 0, label + " support identity baseline dedupe reset");
        foreach (string field in new[] { "selection", "armourSelection" })
        {
            var selection = (EnemySelection)Get(form, field);
            Check(Get(selection, "match") == null && selection.Includes(Enemy()), label + " " + field + " reset");
        }
        var armour = (ArmourProductionTracker)Get(form, "armourAlerts");
        Check(Get(armour, "match") == null && ((HashSet<string>)Get(armour, "initialized")).Count == 0
            && ((HashSet<string>)Get(armour, "seen")).Count == 0, label + " armour identity initialized seen reset");
        var alerts = (AlertTracker)Get(form, "alerts");
        Check(!(bool)Get(alerts, "initialized"), label + " alerts baseline reset");
        foreach (string field in new[] { "enemyBarrages", "barrageOwners", "grenades", "groups", "completed", "confirmations", "artillery" })
        {
            object collection = Get(alerts, field);
            Check((int)collection.GetType().GetProperty("Count").GetValue(collection) == 0, label + " alerts " + field + " reset");
        }
        var prefs = (OverlayPreferences)Get(form, "preferences");
        Check(!prefs.Enabled && prefs.FontSize == 23 && !prefs.ShowMines, label + " persistent preferences retained");
        if (Has("supportActive49"))
        {
            Check(((Dictionary<string, SupportCommand49>)Get(form, "supportActive49")).Count == 0, label + " active support reset");
            Check(!(bool)Get(form, "flankActive47") && !(bool)Get(form, "flankWasDown47") && Get(form, "flankMatch47") == null
                && ((List<SquadMarker>)Get(form, "flankUnits47")).Count == 0 && Get(form, "flankResult47") == null, label + " flank active identity units reset");
            Check(new[] { "flankArmed47", "flankFresh47", "flankNextSound47" }.All(f => (DateTime)Get(form, f) == DateTime.MinValue)
                && new[] { "flankX47", "flankY47", "flankZ47" }.All(f => (float)Get(form, f) == 0)
                && (long)Get(form, "flankSource47") == 0 && (long)Get(form, "flankIdentity47") == 0, label + " flank timing coordinates source reset");
            Check((bool)Get(form, "flankEnabled47") && !(bool)Get(form, "flankRange52")
                && (int)Get(form, "flankRadius47") == 77 && (string)Get(form, "flankKey47") == "F11", label + " flank preferences retained");
        }
    }

    private static void Lifecycle(object form)
    {
        Seed(form, "A");
        Invoke(form, "ClearSessionData");
        CheckReset(form, "clear");
        Invoke(form, "ClearSessionData");
        CheckReset(form, "repeat-clear");

        Seed(form, "A");
        object previousReader = ReaderIdentity();
        Set(form, "reader", previousReader);
        Invoke(form, "DropReader");
        CheckReset(form, "detach");
        Check(Get(form, "reader") == null && (int)Get(form, "pid") == 0, "detach reader and pid cleared");
        Check((bool)Get(previousReader, "miniDisposed"), "detach disposes previous handle-free reader and invalidates its scan epoch");
        object nextReader = ReaderIdentity();
        Set(form, "reader", nextReader);
        var freshA = new List<PlayerInfoRow> { Enemy() };
        Invoke(form, "AcceptSnapshot", freshA, "A");
        Check(ReferenceEquals(Get(form, "reader"), nextReader) && !(bool)Get(nextReader, "miniDisposed"), "same-key reconnect keeps new reader alive");
        Check(ReferenceEquals(Get(form, "rows"), freshA) && freshA.Count == 1, "same-key reconnect keeps successful fresh rows");
        Check(((EnemySelection)Get(form, "selection")).Includes(freshA[0]), "same-key reconnect clears old selection");
        var armour = (ArmourProductionTracker)Get(form, "armourAlerts");
        var armourSelection = (EnemySelection)Get(form, "armourSelection");
        Check(armour.Update("A", freshA, armourSelection).Count == 0, "same-key reconnect first sample silent baseline");
        freshA[0].Producing.Add(Job());
        Check(armour.Update("A", freshA, armourSelection).Count == 1, "same-key reconnect reused job alerts after new baseline");
        Check(armour.Update("A", freshA, armourSelection).Count == 0, "same-key reconnect job dedupe retained within session");
        Check(((HashSet<string>)Get(form, "supportSeen49")).Add("reused-support") && !(bool)Get(form, "supportBaseline49"), "same-key reconnect support key reusable baseline not armed");

        Seed(form, "A");
        var same = new List<PlayerInfoRow> { Enemy() };
        Invoke(form, "AcceptSnapshot", same, "A");
        Check(!((EnemySelection)Get(form, "selection")).Includes(same[0])
            && ((HashSet<string>)Get(form, "supportSeen49")).Contains("reused-support") && (bool)Get(form, "supportBaseline49"), "same-match polling preserves transient dedupe and choices");

        object liveReader = nextReader;
        Set(form, "reader", liveReader);
        var freshB = new List<PlayerInfoRow> { Enemy() };
        Invoke(form, "AcceptSnapshot", freshB, "B");
        Check(ReferenceEquals(Get(form, "reader"), liveReader) && !(bool)Get(liveReader, "miniDisposed")
            && (int)Get(form, "pid") == 42, "same-reader A to B retains successful reader and pid");
        Check(ReferenceEquals(Get(form, "rows"), freshB) && freshB.Count == 1 && (string)Get(form, "rowsMatch") == "B", "same-reader A to B assigns fresh after clearing old rows");
        Check(((EnemySelection)Get(form, "selection")).Includes(freshB[0]) && ((EnemySelection)Get(form, "armourSelection")).Includes(freshB[0]), "same-reader A to B selections reset before new display");
        Check(((HashSet<string>)Get(form, "supportSeen49")).Count == 0 && Get(form, "supportMatch49") == null
            && !(bool)Get(form, "supportBaseline49"), "same-reader A to B support reset");
        Check(((ArmourProductionTracker)Get(form, "armourAlerts")).Update("B", freshB, armourSelection).Count == 0, "same-reader A to B armour baseline silent");
        Check(((AlertTracker)Get(form, "alerts")).Update("B", freshB, (EnemySelection)Get(form, "selection")).Count == 0, "same-reader A to B alerts baseline silent");
        Set(form, "reader", null); // Uninitialized test identity owns no process or handle.
    }

    private static List<MethodBase> Calls(MethodInfo method)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(OpCode))
            .Select(f => (OpCode)f.GetValue(null)).ToDictionary(c => unchecked((ushort)c.Value));
        byte[] il = method.GetMethodBody().GetILAsByteArray();
        var calls = new List<MethodBase>();
        for (int offset = 0; offset < il.Length;)
        {
            ushort value = il[offset++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
            OpCode code = codes[value];
            if (code.OperandType == OperandType.InlineMethod) calls.Add(method.Module.ResolveMethod(BitConverter.ToInt32(il, offset)));
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
        return calls;
    }

    private static void CheckProductionOrdering()
    {
        var calls = Calls(FormType.GetMethod("Poll", Fields));
        int snapshot = calls.FindIndex(m => m.Name == "Snapshot" && m.DeclaringType == typeof(PlayerInfoReader));
        int accept = calls.FindIndex(m => m.Name == "AcceptSnapshot" && m.DeclaringType == FormType);
        int publish = calls.FindIndex(m => m.Name == "Layer55" && m.DeclaringType == FormType);
        Check(snapshot >= 0 && accept > snapshot && publish > accept, "production Poll snapshot then AcceptSnapshot before first overlay publication");
        Check(Calls(FormType.GetMethod("DropReader", Fields)).Any(m => m.Name == "ClearSessionData"), "production DropReader calls actual reset transaction");
        Check(!Calls(FormType.GetMethod("AcceptSnapshot", Fields)).Any(m => m.Name == "Dispose"), "production AcceptSnapshot has no reader disposal");
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            Console.WriteLine("Fixture=" + FormType.Name + "; level=F; no form constructors, tracking, process discovery, game native or credentials");
            object form = NewForm();
            Lifecycle(form);
            CheckProductionOrdering();
            var controls = Owned.OfType<Control>()
                .Concat(Owned.OfType<SquadMarkerHost>().Select(h => (Control)Get(h, "window")))
                .Concat(Owned.OfType<AirOverlayHost>().Select(h => (Control)Get(h, "window")));
            Check(controls.All(c => !c.IsHandleCreated), "fixture created no top-level or control window handles");
            Check(Owned.OfType<SquadMarkerHost>().All(h => Get(h, "thread") == null)
                && Owned.OfType<AirOverlayHost>().All(h => Get(h, "thread") == null), "fixture started no renderer threads");
            Console.WriteLine("RESULT PASS assertions=" + assertions);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
        finally
        {
            for (int i = Owned.Count - 1; i >= 0; i--) Owned[i].Dispose();
        }
    }
}
