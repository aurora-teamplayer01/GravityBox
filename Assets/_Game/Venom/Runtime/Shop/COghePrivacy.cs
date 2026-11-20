using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Local-only age band and optional adult analytics choice. Never sent as telemetry.</summary>
    public static class COghePrivacy
    {
        public enum AgeBand { Unknown = 0, Under13 = 1, Age13To15 = 2, Age16To17 = 3, Adult = 4, Under9 = 5, Age9To12 = 6 }
        private const string Key = "alienpet.privacy.v1";
        [Serializable] private sealed class Choice
        {
            public int version = 3;
            public AgeBand age;
            public bool analyticsChoiceMade, analytics;
            public bool adMeasurementChoiceMade, adMeasurement;
        }
        private static Choice saved, test;
        public static event Action Changed;
        /// <summary>Invalidates asynchronous work after any choice change, including revoke then regrant.</summary>
        public static int Revision { get; private set; }
        private static Choice Current
        {
            get
            {
                if (test != null) return test;
                if (saved != null) return saved;
                try { saved = JsonUtility.FromJson<Choice>(PlayerPrefs.GetString(Key, "")); }
                catch (ArgumentException) { }
                saved = Upgrade(saved);
                if (saved.age != AgeBand.Adult) { saved.analytics = false; saved.analyticsChoiceMade = saved.age != AgeBand.Unknown; }
                return saved;
            }
        }
        // Disclosure v2 includes Tenjin install attribution. Keep age and a prior refusal,
        // but never carry an old affirmative choice into the expanded analytics purpose.
        private static Choice Upgrade(Choice choice)
        {
            if (choice == null || !Valid(choice.age) || choice.version < 1 || choice.version > 3) return new Choice();
            if (choice.version == 1)
            {
                if (choice.age == AgeBand.Adult && choice.analytics)
                { choice.analytics = false; choice.analyticsChoiceMade = false; }
                choice.version = 2;
            }
            if (choice.version < 3)
            {
                // Existing Analytics consent never authorizes advertising identifiers.
                choice.adMeasurement = false;
                choice.adMeasurementChoiceMade = false;
                choice.version = 3;
            }
            if (choice.age != AgeBand.Adult)
            {
                choice.adMeasurement = false;
                choice.adMeasurementChoiceMade = false;
            }
            return choice;
        }
        /// <summary>Memory-only migration fixture for regression tests.</summary>
        public static void LoadChoiceForTests(string json)
        {
            test = Upgrade(JsonUtility.FromJson<Choice>(json));
            Revision++; Changed?.Invoke();
        }
        private static bool Valid(AgeBand age) => age >= AgeBand.Unknown && age <= AgeBand.Age9To12;
        public static AgeBand Age => Current.age;
        public static bool AgeKnown => Age != AgeBand.Unknown;
        public static bool CanChooseAnalytics => Age == AgeBand.Adult;
        public static bool AnalyticsChoiceMade => Current.analyticsChoiceMade;
        public static bool NeedsAgeRefinement => Age == AgeBand.Under13;
        public static bool SetupComplete => AgeKnown && !NeedsAgeRefinement && AnalyticsChoiceMade;
        public static bool AnalyticsAllowed => SetupComplete && CanChooseAnalytics && Current.analytics;
        // Under13 is a legacy unresolved band; never silently promote it to child ads.
        public static bool IsChild => Age == AgeBand.Age9To12;
        // This rollout is Android only. iOS keeps the existing under-13 block.
        public static bool ChildBannerPlatformSupported => Application.platform == RuntimePlatform.Android || Application.isEditor;
        public static bool AdsAgeEligible => SetupComplete && ((IsChild && ChildBannerPlatformSupported) || IsTeen || Age == AgeBand.Adult);
        public static bool FullScreenAdsEligible => AdsAgeEligible && !IsChild;
        public static bool BannerPlacementAllowed(string placement) => AdsAgeEligible &&
            (!IsChild || placement == "menu" || placement == "mainmenu" || placement == "home");
        public static bool IsTeen => Age == AgeBand.Age13To15 || Age == AgeBand.Age16To17;
        // First rollout is an isolated Android attribution QA build. Production stays closed
        // until shared-SDK native tests, public disclosures and Play attribution are verified.
        public static bool? AdMeasurementFeatureForTests;
        public static bool AdMeasurementFeatureEnabled
        {
            get
            {
                if (Application.isEditor && AdMeasurementFeatureForTests.HasValue) return AdMeasurementFeatureForTests.Value;
#if UNITY_ANDROID && ALIENPET_AD_MEASUREMENT_QA
                return true;
#else
                return false;
#endif
            }
        }
        public static bool CanChooseAdMeasurement => AdMeasurementFeatureEnabled && Age == AgeBand.Adult;
        public static bool AdMeasurementChoiceMade => Current.adMeasurementChoiceMade;
        public static bool NeedsAdMeasurementChoice => CanChooseAdMeasurement && AnalyticsChoiceMade && !AdMeasurementChoiceMade;
        public static bool AdMeasurementAllowed => CanChooseAdMeasurement && SetupComplete && Current.adMeasurementChoiceMade && Current.adMeasurement;
        public static bool AdMeasurementPermitValid(int revision) => revision == Revision && AdMeasurementAllowed;
        public static void SetAdMeasurementChoice(bool allowed)
        {
            if (!CanChooseAdMeasurement) return;
            var choice = Current;
            if (choice.adMeasurementChoiceMade && choice.adMeasurement == allowed) return;
            choice.adMeasurementChoiceMade = true;
            choice.adMeasurement = allowed;
            SaveAndNotify();
        }
        public static bool AnalyticsPermitValid(int revision) => revision == Revision && AnalyticsAllowed;
        public static bool AdsPermitValid(int revision) => revision == Revision && AdsAgeEligible;

        public static void SetAgeBand(AgeBand age)
        {
            if (!Valid(age)) throw new ArgumentOutOfRangeException(nameof(age));
            var choice = Current;
            if (choice.age == age) return;
            choice.age = age;
            choice.analytics = false;
            choice.adMeasurement = false;
            choice.adMeasurementChoiceMade = false;
            choice.analyticsChoiceMade = age != AgeBand.Unknown && age != AgeBand.Adult;
            SaveAndNotify();
        }
        public static void SetAnalyticsChoice(bool allowed)
        {
            if (!CanChooseAnalytics) return;
            var choice = Current;
            if (choice.analyticsChoiceMade && choice.analytics == allowed) return;
            choice.analyticsChoiceMade = true; choice.analytics = allowed;
            SaveAndNotify();
        }
        private static void SaveAndNotify()
        {
            if (test == null) { PlayerPrefs.SetString(Key, JsonUtility.ToJson(Current)); PlayerPrefs.Save(); }
            Revision++; Changed?.Invoke();
        }
        public static void ResetChoices()
        {
            if (test != null) test = new Choice();
            else { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); saved = new Choice(); }
            Revision++; Changed?.Invoke();
        }
        /// <summary>Memory-only test fixture; never touches a player's saved age or consent.</summary>
        public static void ResetForTests(AgeBand age = AgeBand.Unknown, bool? analytics = null)
        {
            if (!Valid(age)) throw new ArgumentOutOfRangeException(nameof(age));
            test = new Choice { age = age, analyticsChoiceMade = age != AgeBand.Unknown && (age != AgeBand.Adult || analytics.HasValue), analytics = age == AgeBand.Adult && analytics == true };
            Revision++; Changed?.Invoke();
        }
        public static void ClearTestOverride() { AdMeasurementFeatureForTests = null; test = null; saved = null; Revision++; Changed?.Invoke(); }
    }
}
