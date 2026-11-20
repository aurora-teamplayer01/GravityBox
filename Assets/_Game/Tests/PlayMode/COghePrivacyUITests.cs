using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        private IEnumerator PrivacyColdStart(bool seen=false,bool legacy=false)
        {
            COghePrivacy.ResetForTests();COgheProductUI.PrivacyGateForTests=true;
            yield return ColdStart(seen,legacy);
        }

        [UnityTest] public IEnumerator PrivacyUnknownBlocksIntroWorldAndBackUntilAnExplicitNeutralAgeChoice()
        {
            yield return PrivacyColdStart();
            Assert.AreEqual(COgheProductPage.PrivacySetup,ui.Page);
            Assert.IsFalse(COgheIntro.Playing);Assert.IsTrue(game.Owner.Paused);
            Assert.IsTrue(ui.BlockWorldInput);Assert.IsTrue(ui.HandleInput());
            ui.Play();ui.Resume();ui.ShowMenu();ui.ShowPopup(COgheProductPopup.Pause);
            Assert.AreEqual(COgheProductPage.PrivacySetup,ui.Page,"No navigation bypasses setup");
            var ages=ui.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Age ")).ToArray();
            Assert.AreEqual(5,ages.Length);
            foreach(var b in ages)
            {
                Assert.AreEqual(((RectTransform)ages[0].transform).rect.size,((RectTransform)b.transform).rect.size);
                Assert.AreEqual(ages[0].image.color,b.image.color,"No age group is visually favoured");
                Assert.GreaterOrEqual(((RectTransform)b.transform).rect.height,48);
            }
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Confirm age").interactable);
            float before=game.Matter.SimulationTime;yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(before,game.Matter.SimulationTime);
            string opened=null;COgheProductUI.PrivacyOpenUrlForTests=url=>opened=url;
            yield return Click("Privacy policy");Assert.AreEqual(COgheProductUI.PrivacyPolicyUrl,opened);
            Assert.IsFalse(COghePrivacy.AgeKnown,"Reading the policy doesn't choose an age");
            yield return Click("Age Under9");Assert.IsFalse(COghePrivacy.AgeKnown,"Confirm before persisting an age");
            yield return Click("Confirm age");
            Assert.AreEqual(COghePrivacy.AgeBand.Under9,COghePrivacy.Age);
            Assert.IsTrue(COghePrivacy.SetupComplete);Assert.IsFalse(COghePrivacy.AnalyticsAllowed);Assert.IsFalse(COghePrivacy.AdsAgeEligible);
            Assert.AreEqual(COgheProductPage.Intro,ui.Page,"First launch continues to intro, without a Play gate");
            var intro=Object.FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Skip();
            yield return null;Assert.AreEqual(COgheProductPage.Game,ui.Page);Assert.IsFalse(game.Owner.Paused);
            ui.ShowVictoryForTests(game.Definition.Id,1);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Triple drops"),"Protected players never see an unusable watch-ad prompt");
        }

        [UnityTest] public IEnumerator LegacyUnder13RefinementOffersOnlyChildBandsWithoutSilentAds()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Under13);
            COgheProductUI.PrivacyGateForTests=true;
            yield return ColdStart(true);
            Assert.AreEqual(COgheProductPage.PrivacySetup, ui.Page);
            Assert.IsFalse(COghePrivacy.AdsAgeEligible);
            var ages=ui.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Age ")).ToArray();
            CollectionAssert.AreEquivalent(new[]{"Age Under9", "Age Age9To12"}, ages.Select(b=>b.name));
            yield return Click("Age Under9"); yield return Click("Confirm age");
            Assert.IsTrue(COghePrivacy.SetupComplete); Assert.IsFalse(COghePrivacy.AdsAgeEligible);
        }

        [UnityTest] public IEnumerator ChildAgeChoiceSkipsAnalyticsAndHidesVideoOffers()
        {
            yield return PrivacyColdStart(true);
            yield return Click("Age Age9To12"); yield return Click("Confirm age");
            Assert.AreEqual(COghePrivacy.AgeBand.Age9To12, COghePrivacy.Age);
            Assert.IsTrue(COghePrivacy.SetupComplete);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Analytics yes"));
            ui.ShowMenu(); yield return null;
            ui.ShowVictoryForTests(game.Definition.Id, 1); yield return null;
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Triple drops"));
            Assert.IsFalse(COgheAds.RewardedAvailable);
        }

        [UnityTest] public IEnumerator PrivacyAdultCanDeclineAnalyticsAndChangeItLaterWithoutChangingAge()
        {
            yield return PrivacyColdStart(true);
            yield return Click("Age Adult");yield return Click("Confirm age");
            Assert.AreEqual(COgheProductPage.PrivacySetup,ui.Page);Assert.IsFalse(COghePrivacy.SetupComplete);
            var no=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Analytics no");
            var yes=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Analytics yes");
            Assert.AreEqual(((RectTransform)no.transform).rect.size,((RectTransform)yes.transform).rect.size);
            Assert.AreEqual(no.image.color,yes.image.color);Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            yield return Click("Analytics no");
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsTrue(COghePrivacy.SetupComplete);Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            COgheAds.PrivacyRequired=()=>false;
            yield return Click("Privacy");Assert.AreEqual(COgheProductPopup.Privacy,ui.Popup);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name.StartsWith("Age ")),"No easy age-up shortcut");
            Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Privacy policy"));
            yield return Click("Analytics choice");Assert.IsTrue(COghePrivacy.AnalyticsAllowed);
            yield return Click("Analytics choice");Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            Assert.AreEqual(COghePrivacy.AgeBand.Adult,COghePrivacy.Age);
            yield return Click("Privacy done");Assert.AreEqual(COgheProductPopup.None,ui.Popup);
            ui.Play();yield return null;yield return Click("Pause");yield return Click("Privacy");
            Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Privacy policy"),"Policy accessible without a UMP form");
            yield return Click("Privacy done");Assert.AreEqual(COgheProductPopup.Pause,ui.Popup);
        }

        [UnityTest] public IEnumerator PrivacyLegacySaveNeedsAgeOnceAndTeenContinuesWithoutAnAnalyticsRequest()
        {
            yield return PrivacyColdStart(false,true);
            Assert.AreEqual(COgheProductPage.PrivacySetup,ui.Page);
            yield return Click("Age Age13To15");yield return Click("Confirm age");
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsFalse(COgheIntro.Playing);
            Assert.AreEqual(1,game.Progress.Completed.Count);Assert.AreEqual(2,ui.ResumeLevel);
            Assert.IsTrue(COghePrivacy.SetupComplete);Assert.IsFalse(COghePrivacy.AnalyticsAllowed);Assert.IsFalse(COghePrivacy.CanChooseAnalytics);
            yield return ColdStart(true);
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page,"Known local choice does not repeat setup");
            yield return Click("Privacy");
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Analytics choice"));
        }

        [UnityTest] public IEnumerator AdultMeasurementIsSeparateOptionalAndRevocableThroughRealButtons()
        {
            yield return PrivacyColdStart(true);
            COghePrivacy.AdMeasurementFeatureForTests=true;
            yield return Click("Age Adult");yield return Click("Confirm age");yield return Click("Analytics yes");
            Assert.IsTrue(COghePrivacy.AnalyticsAllowed);Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
            Assert.AreEqual(COgheProductPage.PrivacySetup,ui.Page);
            var no=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Ad measurement no");
            var yes=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Ad measurement yes");
            Assert.AreEqual(no.image.color,yes.image.color);
            Assert.AreEqual(((RectTransform)no.transform).rect.size,((RectTransform)yes.transform).rect.size);
            var copy=ui.GetComponentsInChildren<Text>().Single(t=>t.name=="Disclosure");
            StringAssert.Contains("Google Advertising ID",copy.text);StringAssert.Contains("Unity Ads",copy.text);
            try
            {
                foreach(var size in new[]{new Vector2(360,640),new Vector2(800,1280),new Vector2(1280,800)})
                {
                    COgheProductUI.LayoutAreaForTests=new Rect(Vector2.zero,size);ui.RelayoutForTests();Canvas.ForceUpdateCanvases();
                    foreach(var button in ui.GetComponentsInChildren<Button>())
                    {
                        var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                        foreach(var corner in corners)
                        {
                            var point=ui.SafeRoot.InverseTransformPoint(corner);
                            Assert.That(point.x,Is.InRange(-.1f,ui.SafeRoot.rect.width+.1f),button.name);
                            Assert.That(point.y,Is.InRange(-ui.SafeRoot.rect.height-.1f,.1f),button.name);
                        }
                    }
                }
            }
            finally{COgheProductUI.LayoutAreaForTests=null;ui.RelayoutForTests();}
            AdultMeasurementSnapshot();
            yield return Click("Ad measurement no");
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
            Assert.IsTrue(COghePrivacy.AnalyticsAllowed);
            COgheAds.PrivacyRequired=()=>false;
            yield return Click("Privacy");yield return Click("Ad measurement choice");
            Assert.IsFalse(COghePrivacy.AdMeasurementAllowed,"Review is not consent");
            yield return Click("Ad measurement yes");Assert.IsTrue(COghePrivacy.AdMeasurementAllowed);
            yield return Click("Ad measurement choice");Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
            Assert.IsTrue(COghePrivacy.AnalyticsAllowed);
        }

        [UnityTest] public IEnumerator TeenNeverSeesAdMeasurementPromptEvenInQaBuild()
        {
            yield return PrivacyColdStart(true);COghePrivacy.AdMeasurementFeatureForTests=true;
            yield return Click("Age Age13To15");yield return Click("Confirm age");
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);
            yield return Click("Privacy");
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name.StartsWith("Ad measurement")));
        }

        private void AdultMeasurementSnapshot()
        {
            const string dir="Artifacts/AlienPetAdultAttribution20261119";System.IO.Directory.CreateDirectory(dir);
            var rt=new RenderTexture(720,1280,24);var tex=new Texture2D(720,1280,TextureFormat.RGB24,false);
            var canvas=ProductCanvas;var camera=game.Owner.View;var previous=RenderTexture.active;
            var aspect=camera.aspect;
            try
            {
                COgheProductUI.LayoutAreaForTests=new Rect(0,0,720,1280);ui.RelayoutForTests();
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.01f;
                camera.aspect=720f/1280f;camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,720,1280),0,0);tex.Apply();
                System.IO.File.WriteAllBytes(dir+"/adult-consent.png",tex.EncodeToPNG());
            }
            finally{RenderTexture.active=previous;camera.targetTexture=null;camera.aspect=aspect;canvas.renderMode=RenderMode.ScreenSpaceOverlay;COgheProductUI.LayoutAreaForTests=null;ui.RelayoutForTests();Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);}
        }

        [UnityTest] public IEnumerator PrivacySetupKeepsAllTouchTargetsInsidePhoneAndTabletSafeAreas()
        {
            yield return PrivacyColdStart();
            try
            {
                foreach(var size in new[]{new Vector2(360,640),new Vector2(800,1280),new Vector2(1280,800)})
                {
                    COgheProductUI.LayoutAreaForTests=new Rect(Vector2.zero,size);ui.RelayoutForTests();Canvas.ForceUpdateCanvases();
                    foreach(var button in ui.GetComponentsInChildren<Button>())
                    {
                        var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                        foreach(var corner in corners)
                        {
                            var p=ui.SafeRoot.InverseTransformPoint(corner);
                            Assert.That(p.x,Is.InRange(-.1f,ui.SafeRoot.rect.width+.1f),button.name);
                            Assert.That(p.y,Is.InRange(-ui.SafeRoot.rect.height-.1f,.1f),button.name);
                        }
                    }
                }
            }
            finally{COgheProductUI.LayoutAreaForTests=null;ui.RelayoutForTests();}
        }
    }
}
