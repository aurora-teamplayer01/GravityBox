#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native UI review. Does not change the player's saved progress or owned items.</summary>
    public sealed class AlienPetBrandProof : MonoBehaviour
    {
        private static string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-alien-pet-brand-proof");
            if (at < 0 || at + 1 >= args.Length) return;
            output = Path.GetFullPath(args[at + 1]);
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            COgheIntro.SeenOverrideForTests = true;
            DontDestroyOnLoad(new GameObject("Alien Pet brand review").AddComponent<AlienPetBrandProof>().gameObject);
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(output); Application.runInBackground = true;
            yield return new WaitForSecondsRealtime(2);
            var game = FindFirstObjectByType<VenomCampaign>(); var ui = game.ProductUI;
            ui.ShowMenu();
            var sizes = new[] { new Vector2Int(540, 960), new Vector2Int(540, 1170), new Vector2Int(768, 1024) };
            foreach (var size in sizes)
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(2); ui.RelayoutForTests(); Canvas.ForceUpdateCanvases();
                var title = ui.GetComponentsInChildren<Text>().Single(t => t.name == "Game logo");
                if (title.preferredWidth > title.rectTransform.rect.width)
                    throw new InvalidOperationException("Game title wraps at " + size);
                yield return Capture("menu-" + size.x + "x" + size.y);
            }
            ui.ShowPopup(COgheProductPopup.Plus); yield return Capture("plus-tablet"); ui.Resume();
            ui.Play(); yield return new WaitForSecondsRealtime(1); yield return Capture("game-tablet");
            ui.ShowPopup(COgheProductPopup.Pause); yield return Capture("pause-tablet");
            File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: native menu at three aspect ratios; title fits; Plus, gameplay and pause captured. Save persistence disabled.\n");
            Application.Quit();
        }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
    }
}
#endif
