using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Local-only age band and optional adult analytics choice. Never sent as telemetry.</summary>
    public static class COghePrivacy
    {
        public enum AgeBand { Unknown, Under13, Age13To15, Age16To17, Adult }
        private const string Key = "alienpet.privacy.v1";
        [Serializable] private sealed class Choice
        {
            public int version = 1;
            public AgeBand age;
            public bool analyticsChoiceMade, analytics;
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
                if (saved == null || saved.version != 1 || !Valid(saved.age)) saved = new Choice();
                if (saved.age != AgeBand.Adult) { saved.analytics = false; saved.analyticsChoiceMade = saved.age != AgeBand.Unknown; }
                return saved;
            }
        }
        private static bool Valid(AgeBand age) => age >= AgeBand.Unknown && age <= AgeBand.Adult;
        public static AgeBand Age => Current.age;
        public static bool AgeKnown => Age != AgeBand.Unknown;
        public static bool CanChooseAnalytics => Age == AgeBand.Adult;
        public static bool AnalyticsChoiceMade => Current.analyticsChoiceMade;
        public static bool SetupComplete => AgeKnown && AnalyticsChoiceMade;
        public static bool AnalyticsAllowed => SetupComplete && CanChooseAnalytics && Current.analytics;
        public static bool AdsAgeEligible => SetupComplete && Age != AgeBand.Under13;
        public static bool IsTeen => Age == AgeBand.Age13To15 || Age == AgeBand.Age16To17;
        public static bool AnalyticsPermitValid(int revision) => revision == Revision && AnalyticsAllowed;
        public static bool AdsPermitValid(int revision) => revision == Revision && AdsAgeEligible;

        public static void SetAgeBand(AgeBand age)
        {
            if (!Valid(age)) throw new ArgumentOutOfRangeException(nameof(age));
            var choice = Current;
            if (choice.age == age) return;
            choice.age = age;
            choice.analytics = false;
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
        public static void ClearTestOverride() { test = null; saved = null; Revision++; Changed?.Invoke(); }
    }
}
