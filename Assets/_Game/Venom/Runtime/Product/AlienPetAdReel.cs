#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Text;
using GravityBox.Venom.ChapterProof;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Offline source footage: normal touch commands and runtime physics; no saved progression.</summary>
    public sealed class AlienPetAdReel : MonoBehaviour
    {
        private static string output;
        private int frame; private bool recording;
        private readonly StringBuilder marks = new StringBuilder();
        private VenomCampaign game;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-alien-pet-ad-reel");
            if (at < 0 || at + 1 >= args.Length) return;
            output = Path.GetFullPath(args[at + 1]);
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            COgheIntro.SeenOverrideForTests = true;
            DontDestroyOnLoad(new GameObject("Alien Pet ad source capture").AddComponent<AlienPetAdReel>().gameObject);
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(output); Screen.SetResolution(540, 960, FullScreenMode.Windowed);
            Application.runInBackground = true; yield return new WaitForSecondsRealtime(2);
            game = FindFirstObjectByType<VenomCampaign>();
            StartCoroutine(Capture());
            foreach (var level in new[] { 4, 6 })
            {
                game.ProductUI.LoadForTest(level); yield return new WaitForSecondsRealtime(2);
                game = FindFirstObjectByType<VenomCampaign>();
                game.AutoAdvance = false; Time.captureFramerate = 30; recording = true;
                Mark("level " + level + " start"); yield return Frames(30);
                yield return new COgheSpatialScenario(game, Tap, Until).Solve();
                if (!game.Owner.Completed) throw new InvalidOperationException("Capture solve failed: " + level);
                Mark("level " + level + " solved"); yield return Frames(48);
                recording = false; Time.captureFramerate = 0;
            }
            // Showcase an unlocked Home without writing purchases or campaign progress.
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true });
            COgheHomeRoom.UnlockedLevelOverride = 32; game.Progress.HomeUnlocked = true;
            game.ProductUI.ShowMenu(); game.ProductUI.OpenHome(); yield return new WaitForSecondsRealtime(5);
            game.ProductUI.ToggleHomeZoom(); yield return new WaitForSecondsRealtime(2);
            Time.captureFramerate = 30; recording = true; Mark("home"); yield return Frames(30);
            for (int i = 0; i < 3; i++)
            { yield return Tap(game.Personality.SkinCentre + Vector3.up * .02f); yield return Frames(i < 2 ? 35 : 100); }
            Mark("home affection"); yield return Frames(40);
            game.ProductUI.ToggleHomeZoom(); yield return Frames(40);
            game.ProductUI.ShowMenu(); Mark("brand menu"); yield return Frames(75);
            recording = false; Time.captureFramerate = 0;
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            yield return new WaitForSecondsRealtime(2); Application.Quit();
        }
        private void Mark(string message) { marks.AppendLine(frame + " " + message); File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString()); }
        private IEnumerator Tap(Vector3 world)
        { Mark("tap"); game.TouchPoint(game.Owner.View.WorldToScreenPoint(world)); yield return Frames(2); }
        private IEnumerator Until(float seconds, Func<bool> done, string reason)
        {
            Mark(reason); int limit = Mathf.CeilToInt(seconds * 30);
            for (int i = 0; i < limit && !done(); i++) yield return null;
            if (!done()) { recording = false; File.WriteAllText(Path.Combine(output, "FAILED.txt"), reason); Application.Quit(1); }
        }
        private IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }
        private IEnumerator Capture()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!recording) continue;
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "frame-" + frame.ToString("D5") + ".png"), 2);
                if (++frame > 4000) { recording = false; File.WriteAllText(Path.Combine(output, "FAILED.txt"), "Frame limit"); Application.Quit(1); }
            }
        }
    }
}
#endif
