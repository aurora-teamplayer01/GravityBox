#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in marketing capture of real Home, food and wardrobe interactions. Writes no player saves.</summary>
    public sealed class AlienPetCareReel : MonoBehaviour
    {
        private static string output;
        private int frame;
        private bool recording;
        private readonly StringBuilder marks = new StringBuilder();
        private readonly StringBuilder sounds = new StringBuilder();
        private VenomCampaign game;
        private COgheProductUI ui;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-alien-pet-care-reel");
            if(at<0||at+1>=args.Length)return;
            output=Path.GetFullPath(args[at+1]);
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            COgheIntro.SeenOverrideForTests=true;
            COgheShop.ResetForTests(new COgheShop.State { Migrated=true,Drops=100,EnergyUsed=50,InkGuideStep=4,InkGiftRevision=1 });
            COgheStyle.ResetForTests(new COgheStyle { Seed=4 });
            DontDestroyOnLoad(new GameObject("Alien Pet care source capture").AddComponent<AlienPetCareReel>().gameObject);
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(output);Screen.SetResolution(540,960,FullScreenMode.Windowed);
            Application.runInBackground=true;yield return new WaitForSecondsRealtime(2);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            COgheHomeRoom.UnlockedLevelOverride=8;game.Progress.HomeUnlocked=true;
            ui.ShowMenu();ui.OpenHome();yield return new WaitForSecondsRealtime(3);ui.ToggleHomeZoom();yield return new WaitForSecondsRealtime(2);
            StartCoroutine(Capture());COgheAudio.Heard+=Heard;
            recording=true;Time.captureFramerate=30;Mark("PET");yield return Frames(30);
            for(int i=0;i<3;i++){game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Personality.SkinCentre));yield return Frames(i<2?30:110);}
            Mark("FEED_APPLE");ui.ShowPopup(COgheProductPopup.Food);yield return Frames(30);Click(COgheProductUI.SnackButton("SNACK_APPLE_PIE"));yield return Frames(145);
            Mark("FEED_CHILLI");ui.ShowPopup(COgheProductPopup.Food);yield return Frames(25);Click(COgheProductUI.SnackButton("SNACK_CHILLI_CANDY"));yield return Frames(165);
            if(game.Personality.SnacksEaten!=2){Fail("Two real snack interactions did not finish");yield break;}
            recording=false;Time.captureFramerate=0;
            COgheHomeRoom.UnlockedLevelOverride=38;ui.OpenStyle();yield return new WaitForSecondsRealtime(2);
            recording=true;Time.captureFramerate=30;Mark("COLOR_MINT");yield return Frames(25);ui.ChooseStyleItem("INK_MINT");yield return Frames(10);
            var points=game.Matter.GetComponent<VenomSurface>().DrawnParticles;var centre=Vector3.zero;foreach(var p in points)centre+=p;centre/=points.Length;
            if(!ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(centre),false)){Fail("Mint syringe did not touch body");yield break;}
            yield return Frames(85);ui.EndStyleHold();yield return Frames(45);
            Mark("FLOWER_CROWN");ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_FLOWER");yield return Frames(85);
            Mark("PARTY_HAT");ui.ChooseStyleItem("HAT_PARTY");yield return Frames(65);
            Mark("BEE_HAT");ui.ChooseStyleItem("HAT_BEE");yield return Frames(85);
            Mark("HOME_STYLED");ui.LeaveStyle();yield return Frames(70);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Personality.SkinCentre));yield return Frames(100);
            recording=false;Time.captureFramerate=0;COgheAudio.Heard-=Heard;
            File.WriteAllText(Path.Combine(output,"sounds.txt"),sounds.ToString());
            File.WriteAllText(Path.Combine(output,"frames.txt"),frame.ToString());
            File.WriteAllText(Path.Combine(output,"PASS.txt"),"Actual Home petting, two snacks consumed, Mint hold and three hats recorded; in-memory unlock/ownership fixture, saves disabled.\n");
            yield return new WaitForSecondsRealtime(1);Application.Quit();
        }
        private void Click(string name)
        {
            var button=ui.GetComponentsInChildren<Button>().SingleOrDefault(b=>b.name==name);
            if(button==null){Fail("Missing UI button: "+name);return;}button.onClick.Invoke();
        }
        private void Heard(string clip,float volume){if(recording)sounds.AppendLine($"{frame/30f:F3} {clip} {volume:F2}");}
        private void Mark(string text){marks.AppendLine(frame+" "+text);File.WriteAllText(Path.Combine(output,"marks.txt"),marks.ToString());}
        private IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
        private IEnumerator Capture()
        {
            while(true){yield return new WaitForEndOfFrame();if(!recording)continue;
                ScreenCapture.CaptureScreenshot(Path.Combine(output,$"frame-{frame:D5}.png"),2);
                if(++frame>1800){Fail("Capture frame limit");yield break;}}
        }
        private void Fail(string reason){recording=false;File.WriteAllText(Path.Combine(output,"FAILED.txt"),reason);Application.Quit(1);}
        private void OnDestroy(){COgheAudio.Heard-=Heard;}
    }
}
#endif
