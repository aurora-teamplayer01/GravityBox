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
            Assert.AreEqual(4,ages.Length);
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
            yield return Click("Age Under13");Assert.IsFalse(COghePrivacy.AgeKnown,"Confirm before persisting an age");
            yield return Click("Confirm age");
            Assert.AreEqual(COghePrivacy.AgeBand.Under13,COghePrivacy.Age);
            Assert.IsTrue(COghePrivacy.SetupComplete);Assert.IsFalse(COghePrivacy.AnalyticsAllowed);Assert.IsFalse(COghePrivacy.AdsAgeEligible);
            Assert.AreEqual(COgheProductPage.Intro,ui.Page,"First launch continues to intro, without a Play gate");
            var intro=Object.FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Skip();
            yield return null;Assert.AreEqual(COgheProductPage.Game,ui.Page);Assert.IsFalse(game.Owner.Paused);
            ui.ShowVictoryForTests(game.Definition.Id,1);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Triple drops"),"Protected players never see an unusable watch-ad prompt");
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
