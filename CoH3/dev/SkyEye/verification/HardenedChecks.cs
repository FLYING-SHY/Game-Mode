using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using CoH3MemoryTool;

internal static class HardenedChecks
{
    static void Require(bool value, string label) { if (!value) throw new Exception(label); Console.WriteLine("PASS " + label); }
    static IEnumerable<Control> Walk(Control root) { foreach (Control control in root.Controls) { yield return control; foreach (var child in Walk(control)) yield return child; } }
    static string Hash(string path) { using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); }
    static void Picture(Form form, string path) { Application.DoEvents(); using (var image = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(image, new Rectangle(0, 0, form.Width, form.Height)); image.Save(path); } }
    public static void Ui(string sandbox)
    {
        Exception failure = null;
        var thread = new Thread(() => { try {
            Application.EnableVisualStyles();
            AudioDependency.Initialize();
            using (var form = new OverlaySettingsForm(true)) {
                form.ShowInTaskbar = false;
                form.Show(); Application.DoEvents();
                var controls = Walk(form).ToArray();
                Require(controls.OfType<Module46>().Count() == 7, "seven modules after renaming");
                Require(controls.OfType<Stack46>().Count() == 3, "three stacks after renaming");
                Require(controls.OfType<Stepper46>().Count() >= 40, "numeric controls retained");
                Require(!controls.OfType<TabControl>().Any(), "single console layout");
                Require(controls.OfType<Action46>().Count(c => c.Text == "音源") == 7, "seven independent audio selectors");
                Picture(form, Path.Combine(sandbox, "main.png"));
                var fold = controls.OfType<Fold46>().Single(c => c.Header.Name == "移动目标连线");
                fold.Header.PerformClick(); Application.DoEvents();
                Require(fold.Body.Visible, "fold expands after event renaming");
                Picture(form, Path.Combine(sandbox, "expanded.png"));
                fold.Header.PerformClick(); Application.DoEvents();
                Require(!fold.Body.Visible, "fold collapses after event renaming");
                using (var stepper = new Stepper46(1, 100, 10, 0)) {
                    stepper.Value = 25.9m; Require(stepper.Value == 26, "numeric rounding");
                    stepper.Value = 1000; Require(stepper.Value == 100, "numeric upper limit");
                    stepper.Value = -5; Require(stepper.Value == 1, "numeric lower limit");
                }
                form.ClientSize = new Size(1180, 740); Application.DoEvents();
                Require(!controls.OfType<Stack46>().Any(c => c.HorizontalScroll.Visible), "compact layout");
                Picture(form, Path.Combine(sandbox, "compact.png"));
                File.WriteAllText(Path.Combine(sandbox, "ui-result.json"), new JavaScriptSerializer().Serialize(new { passed = true, controls = controls.Length, modules = controls.OfType<Module46>().Count(), checks = 12 }));
                form.Close(); Require(form.IsDisposed, "preview closes cleanly");
            }
        } catch (Exception error) { failure = error; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure != null) throw failure;
    }
    public static void Audio(string sandbox)
    {
        AudioDependency.Initialize();
        var temp = Path.Combine(sandbox, "audio-temp"); Directory.CreateDirectory(temp);
        Environment.SetEnvironmentVariable("TEMP", temp); Environment.SetEnvironmentVariable("TMP", temp);
        var sounds = Path.Combine(Path.GetDirectoryName(typeof(AudioClip).Assembly.Location), "sounds");
        var field = typeof(AudioClip).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(f => f.FieldType == typeof(SoundPlayer));
        var rows = new List<object>();
        foreach (var path in Directory.GetFiles(sounds, "*.mp3").OrderBy(p => p)) {
            using (var clip = new AudioClip(path)) {
                var decoded = ((SoundPlayer)field.GetValue(clip)).SoundLocation;
                Require(Path.GetFullPath(decoded).StartsWith(Path.GetFullPath(temp) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "isolated audio cache");
                using (var wav = new AudioClip(decoded)) using (var cached = new AudioClip(path)) {
                    Require(((SoundPlayer)field.GetValue(cached)).SoundLocation == decoded, "audio cache reuse");
                    var bytes = File.ReadAllBytes(decoded);
                    Require(bytes.Length > 44 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WAVE", "decoded RIFF/WAVE");
                    rows.Add(new { name = Path.GetFileName(path), sha256 = Hash(decoded), bytes = bytes.Length });
                }
            }
        }
        Require(rows.Count == 8, "all eight embedded-resolver audio fixtures decoded");
        File.WriteAllText(Path.Combine(sandbox, "audio-result.json"), new JavaScriptSerializer().Serialize(new { passed = true, files = rows.Count, decoded = rows, audiblePlaybackTested = false }));
    }
}
