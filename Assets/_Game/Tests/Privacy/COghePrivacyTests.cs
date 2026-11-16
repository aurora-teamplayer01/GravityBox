using System;
using System.Linq;
using System.Xml.Linq;
using GoogleMobileAds.Api;
using GravityBox.Editor;
using GravityBox.Services;
using GravityBox.Venom;
using NUnit.Framework;
using System.IO;
using UnityEditor.Build;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed class COghePrivacyTests
    {
        [SetUp] public void SetUp() => COghePrivacy.ResetForTests();
        [TearDown] public void TearDown() => COghePrivacy.ClearTestOverride();

        [TestCase(COghePrivacy.AgeBand.Unknown)]
        [TestCase(COghePrivacy.AgeBand.Under13)]
        public void UnknownAndChildCannotEnableAnyNetworkService(COghePrivacy.AgeBand age)
        {
            COghePrivacy.SetAgeBand(age);
            COghePrivacy.SetAnalyticsChoice(true);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            Assert.IsFalse(COghePrivacy.AdsAgeEligible);
            Assert.IsFalse(COghePrivacy.CanChooseAnalytics);
            Assert.AreEqual(age == COghePrivacy.AgeBand.Under13, COghePrivacy.SetupComplete);
        }

        [TestCase(COghePrivacy.AgeBand.Age13To15)]
        [TestCase(COghePrivacy.AgeBand.Age16To17)]
        public void TeenSkipsAnalyticsQuestionAndGetsRestrictedAdConfiguration(COghePrivacy.AgeBand age)
        {
            COghePrivacy.SetAgeBand(age);
            COghePrivacy.SetAnalyticsChoice(true);
            Assert.IsTrue(COghePrivacy.SetupComplete);
            Assert.IsTrue(COghePrivacy.AdsAgeEligible);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            var config = COgheGoogleServices.AdRequestConfiguration();
            Assert.AreEqual(AgeRestrictedTreatment.Teen, config.AgeRestrictedTreatment);
#pragma warning disable CS0618
            Assert.AreEqual(TagForUnderAgeOfConsent.True, config.TagForUnderAgeOfConsent);
#pragma warning restore CS0618
            Assert.AreEqual("G", config.MaxAdContentRating.Value, "The SDK serializes the general-audience rating as G.");
            Assert.AreEqual(PublisherPrivacyPersonalizationState.Disabled, config.PublisherPrivacyPersonalizationState);
        }

        [Test] public void AdultHasNoImplicitConsentAndCanPlayAfterDecliningAnalytics()
        {
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Adult);
            Assert.IsFalse(COghePrivacy.SetupComplete);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            Assert.IsFalse(COghePrivacy.AdsAgeEligible);
            COghePrivacy.SetAnalyticsChoice(false);
            Assert.IsTrue(COghePrivacy.SetupComplete);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            Assert.IsTrue(COghePrivacy.AdsAgeEligible);
            Assert.AreEqual(PublisherPrivacyPersonalizationState.Disabled, COgheGoogleServices.AdRequestConfiguration().PublisherPrivacyPersonalizationState);
        }

        [Test] public void RevocationAndRegrantDoNotResurrectAnOldAsyncRequest()
        {
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Adult);
            COghePrivacy.SetAnalyticsChoice(true);
            int old = COghePrivacy.Revision;
            Assert.IsTrue(COghePrivacy.AnalyticsPermitValid(old));
            COghePrivacy.SetAnalyticsChoice(false);
            Assert.IsFalse(COghePrivacy.AnalyticsPermitValid(old));
            COghePrivacy.SetAnalyticsChoice(true);
            Assert.IsFalse(COghePrivacy.AnalyticsPermitValid(old));
            Assert.IsTrue(COghePrivacy.AnalyticsPermitValid(COghePrivacy.Revision));
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Under13);
            Assert.IsFalse(COghePrivacy.AnalyticsPermitValid(COghePrivacy.Revision));
            Assert.IsFalse(COghePrivacy.AdsPermitValid(old));
        }

        [Test] public void ChangingAgeNeverCarriesAnAdultOptInIntoAnotherBand()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true);
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Age16To17);
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Adult);
            Assert.IsFalse(COghePrivacy.AnalyticsChoiceMade);
            COghePrivacy.ResetChoices();
            Assert.IsFalse(COghePrivacy.AgeKnown);
            Assert.IsFalse(COghePrivacy.AdsAgeEligible);
        }

        [Test] public void InvalidAgeCannotOpenTheGate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => COghePrivacy.SetAgeBand((COghePrivacy.AgeBand)99));
            Assert.IsFalse(COghePrivacy.SetupComplete);
        }

        [Test] public void MemoryFixturesDoNotOverwriteExistingPlayerChoice()
        {
            const string key = "alienpet.privacy.v1";
            bool existed = PlayerPrefs.HasKey(key);
            string before = PlayerPrefs.GetString(key, "");
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Adult);
            COghePrivacy.SetAnalyticsChoice(true);
            COghePrivacy.ResetChoices();
            Assert.AreEqual(existed, PlayerPrefs.HasKey(key));
            Assert.AreEqual(before, PlayerPrefs.GetString(key, ""));
        }

        [Test] public void IOSReleaseRejectsWrongFirebaseAppAndInvalidBuildOptions()
        {
            string config=File.ReadAllText("Assets/GoogleService-Info.plist");
            Assert.DoesNotThrow(()=>COgheIOSBuilder.ValidateFirebaseConfig(config));
            Assert.Throws<BuildFailedException>(()=>COgheIOSBuilder.ValidateFirebaseConfig(config.Replace("com.gravityboxlab.venom", "com.other.app")));
            Assert.Throws<BuildFailedException>(()=>COgheIOSBuilder.ValidateFirebaseConfig(config.Replace("coghe-57f21", "wrong-project")));
            Assert.DoesNotThrow(()=>COgheIOSBuilder.ValidateReleaseOptions("1.0.0", "1", true, false));
            Assert.Throws<BuildFailedException>(()=>COgheIOSBuilder.ValidateReleaseOptions("1.0.0", "0", true, false));
            Assert.Throws<BuildFailedException>(()=>COgheIOSBuilder.ValidateReleaseOptions("invalid", "1", true, false));
            Assert.Throws<BuildFailedException>(()=>COgheIOSBuilder.ValidateReleaseOptions("1.0.0", "1", true, true));
        }
#if UNITY_IOS
        [Test] public void IOSNativeDefaultsStayClosedAndPreserveUnrelatedPlistKeys()
        {
            var plist=new PlistDocument(); plist.Create();
            plist.root.SetString("CFBundleIdentifier", COgheIOSBuilder.BundleId);
            plist.root.SetBoolean("FIREBASE_ANALYTICS_COLLECTION_ENABLED", true);
            COghePrivacyBuild.ConfigureIOSPlist(plist);
            string first=plist.WriteToString();
            COghePrivacyBuild.ConfigureIOSPlist(plist);
            Assert.AreEqual(first,plist.WriteToString());
            foreach(string key in new[]{"FirebaseDataCollectionDefaultEnabled", "FIREBASE_ANALYTICS_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_IDFA_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_IDFV_COLLECTION_ENABLED", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", "GOOGLE_ANALYTICS_REGISTRATION_WITH_AD_NETWORK_ENABLED"})
                Assert.IsFalse(plist.root[key].AsBoolean(),key);
            Assert.IsTrue(plist.root["GADDelayAppMeasurementInit"].AsBoolean());
            Assert.AreEqual(COgheIOSBuilder.BundleId,plist.root["CFBundleIdentifier"].AsString());
            Assert.AreEqual("ca-app-pub-7806638519709442/5181412470",COgheAdMob.RewardedId);
        }
#endif

        [Test] public void AndroidNativeDefaultsDisableAutostartAndAdIdsWithoutRemovingGameplayActivity()
        {
            XNamespace android = "http://schemas.android.com/apk/res/android";
            XNamespace tools = "http://schemas.android.com/tools";
            var document = XDocument.Parse("<manifest xmlns:android='http://schemas.android.com/apk/res/android'><uses-permission android:name='android.permission.INTERNET'/><uses-permission android:name='com.google.android.gms.permission.AD_ID'/><application><activity android:name='game.Activity'/><meta-data android:name='firebase_analytics_collection_enabled' android:value='true'/><provider android:name='com.google.firebase.provider.FirebaseInitProvider' android:authorities='app.firebaseinitprovider'/></application></manifest>");
            COghePrivacyBuild.ConfigureAndroidManifest(document);
            string first = document.ToString();
            COghePrivacyBuild.ConfigureAndroidManifest(document);
            Assert.AreEqual(first, document.ToString(), "Rebuilding is idempotent.");
            var root = document.Root; var app = root.Element("application");
            Assert.AreEqual("game.Activity", (string)app.Element("activity").Attribute(android + "name"));
            Assert.AreEqual("false", (string)app.Elements("meta-data").Single(e => (string)e.Attribute(android + "name") == "firebase_analytics_collection_enabled").Attribute(android + "value"));
            Assert.AreEqual("false", (string)app.Elements("meta-data").Single(e => (string)e.Attribute(android + "name") == "google_analytics_default_allow_analytics_storage").Attribute(android + "value"));
            foreach (string provider in new[] { "com.google.firebase.provider.FirebaseInitProvider", "com.google.android.gms.ads.MobileAdsInitProvider" })
                Assert.AreEqual("remove", (string)app.Elements("provider").Single(e => (string)e.Attribute(android + "name") == provider).Attribute(tools + "node"));
            foreach (var permission in root.Elements("uses-permission").Where(e => ((string)e.Attribute(android + "name")).Contains("AD_ID") || ((string)e.Attribute(android + "name")).Contains("ADSERVICES")))
                Assert.AreEqual("remove", (string)permission.Attribute(tools + "node"));
            Assert.IsNull(root.Elements("uses-permission").Single(e => (string)e.Attribute(android + "name") == "android.permission.INTERNET").Attribute(tools + "node"));
        }
    }
}
