using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Android SDK lifetime; game rules depend only on the existing ads / analytics interfaces.</summary>
    public sealed class COgheGoogleServices : MonoBehaviour
    {
        public const string AndroidPackage = "com.gravityboxlab.venom";
        public const string AdMobApp = "ca-app-pub-7806638519709442~4983717950";
        public static bool Ready { get; private set; }
        private FirebaseApp firebase;
        private COgheAdMob ads;
        private bool adsInitializing, firebaseInitializing, dispatcherReady;
        private int consentRevision = -1;
        private float consentRetryDelay = 15;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.identifier != AndroidPackage || Application.isBatchMode) return;
            var host = new GameObject("COghe Google services");
            DontDestroyOnLoad(host); host.AddComponent<COgheGoogleServices>();
#endif
        }

        private void Awake()
        {
            COgheAds.TestAds = false;
            COgheAds.Provider = new COgheNoAds();
            COgheAnalytics.Sink = Send;
            COgheEconomy.LoadCached();
            COgheAds.PrivacyRequired = () => COghePrivacy.AdsAgeEligible && dispatcherReady &&
                ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
            COgheAds.OpenPrivacy = OpenAdPrivacy;
            COghePrivacy.Changed += ApplyPrivacy;
            ApplyPrivacy();
        }

        private void ApplyPrivacy()
        {
            CancelInvoke(nameof(RetryConsent));
            consentRevision = -1;
            ads?.Suspend();
            COgheAds.Provider = new COgheNoAds();
            if (!COghePrivacy.AnalyticsAllowed) StopAnalytics();
            // A fresh blocked session has no native SDK objects to stop. Age-unknown and under-13
            // users stay on local gameplay; changing a prior choice first suspends existing services.
            if (!COghePrivacy.SetupComplete) return;
            if (COghePrivacy.AnalyticsAllowed) StartAnalytics();
            if (!COghePrivacy.AdsAgeEligible) return;
            if (!dispatcherReady) { MobileAdsEventExecutor.Initialize(); dispatcherReady = true; }
            MobileAds.SetRequestConfiguration(AdRequestConfiguration());
            RequestConsent();
        }

        public static RequestConfiguration AdRequestConfiguration()
        {
#pragma warning disable CS0618 // TFUA is retained as the more conservative under-consent-age signal.
            return new RequestConfiguration
            {
                AgeRestrictedTreatment = COghePrivacy.IsTeen ? AgeRestrictedTreatment.Teen : AgeRestrictedTreatment.Unspecified,
                TagForUnderAgeOfConsent = COghePrivacy.IsTeen ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False,
                MaxAdContentRating = MaxAdContentRating.G,
                PublisherPrivacyPersonalizationState = PublisherPrivacyPersonalizationState.Disabled
            };
#pragma warning restore CS0618
        }

        private void StartAnalytics()
        {
            if (!COghePrivacy.AnalyticsAllowed || Ready || firebaseInitializing) return;
            firebaseInitializing = true;
            int ticket = COghePrivacy.Revision;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                firebaseInitializing = false;
                if (this == null) return;
                if (!COghePrivacy.AnalyticsPermitValid(ticket))
                {
                    // Regranting during initialization gets a new permit; never reuse the old one.
                    if (COghePrivacy.AnalyticsAllowed) StartAnalytics();
                    return;
                }
                if (task.IsFaulted || task.IsCanceled || task.Result != DependencyStatus.Available)
                { Debug.LogWarning("[COghe services] Firebase unavailable; gameplay continues."); return; }
                firebase = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
                SetAnalyticsConsent(true);
                FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertyAllowAdPersonalizationSignals, "false");
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetUserProperty("build_channel", COgheTestTools.LevelSelect ? "test" : "store");
                FirebaseAnalytics.SetUserProperty("economy_revision", COgheEconomy.Revision);
                FirebaseAnalytics.SetUserProperty("economy_variant", COgheEconomy.Variant);
                Ready = true;
                // No pre-consent/pre-ready gameplay events are stored for a later upload.
                COgheRemoteConfig.Fetch();
            });
        }

        private static void SetAnalyticsConsent(bool allowAnalytics)
        {
            FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, Firebase.Analytics.ConsentStatus>
            {
                { ConsentType.AnalyticsStorage, allowAnalytics ? Firebase.Analytics.ConsentStatus.Granted : Firebase.Analytics.ConsentStatus.Denied },
                { ConsentType.AdStorage, Firebase.Analytics.ConsentStatus.Denied },
                { ConsentType.AdUserData, Firebase.Analytics.ConsentStatus.Denied },
                { ConsentType.AdPersonalization, Firebase.Analytics.ConsentStatus.Denied }
            });
        }

        private void StopAnalytics()
        {
            Ready = false;
            // Do not call any Firebase static API merely to disable it: that can initialize the SDK.
            if (firebase == null) return;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            SetAnalyticsConsent(false);
            FirebaseAnalytics.ResetAnalyticsData(); // Local SDK data only; not a server-deletion promise.
        }

        private void OpenAdPrivacy()
        {
            if (!COghePrivacy.AdsAgeEligible || !dispatcherReady) return;
            int ticket = COghePrivacy.Revision;
            ads?.Suspend(); COgheAds.Provider = new COgheNoAds();
            ConsentForm.ShowPrivacyOptionsForm(error => Main(() =>
            {
                if (this == null || !COghePrivacy.AdsPermitValid(ticket)) return;
                if (error != null) { ConsentError(error); return; }
                // The separate adult analytics choice remains authoritative; UMP must not opt it in.
                if (firebase != null) SetAnalyticsConsent(COghePrivacy.AnalyticsAllowed);
                EnableAds(ticket);
            }));
        }

        private void RequestConsent()
        {
            if (!COghePrivacy.AdsAgeEligible) return;
            CancelInvoke(nameof(RetryConsent));
            int ticket = COghePrivacy.Revision;
            consentRevision = ticket;
            ConsentInformation.Update(new ConsentRequestParameters { TagForUnderAgeOfConsent = COghePrivacy.IsTeen }, error => Main(() =>
            {
                if (this == null || !COghePrivacy.AdsPermitValid(ticket)) return;
                if (error != null) { ConsentError(error); return; }
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => Main(() =>
                {
                    if (this == null || !COghePrivacy.AdsPermitValid(ticket)) return;
                    if (formError != null) { ConsentError(formError); return; }
                    consentRetryDelay = 15;
                    if (firebase != null) SetAnalyticsConsent(COghePrivacy.AnalyticsAllowed);
                    EnableAds(ticket);
                }));
            }));
        }

        private void ConsentError(FormError error)
        {
            Debug.LogWarning("[COghe services] Consent: " + error.ErrorCode + " " + error.Message);
            // A fresh install can launch offline. Recover in the same session when the network returns.
            // Invalid publisher configuration (3) requires a dashboard fix, not repeated requests.
            if (error.ErrorCode == 3) return;
            Invoke(nameof(RetryConsent), consentRetryDelay);
            consentRetryDelay = Mathf.Min(60, consentRetryDelay * 2);
        }

        private void RetryConsent()
        {
            if (!COghePrivacy.AdsAgeEligible) return;
            if (Application.internetReachability == NetworkReachability.NotReachable)
                Invoke(nameof(RetryConsent), 15);
            else RequestConsent();
        }

        public static void Main(Action action) => MobileAdsEventExecutor.ExecuteInUpdate(action);

        private void EnableAds(int ticket)
        {
            if (!COghePrivacy.AdsPermitValid(ticket) || consentRevision != ticket || !ConsentInformation.CanRequestAds()) return;
            if (ads != null) { COgheAds.Provider = ads; ads.Resume(); COgheAds.RefreshBanner(); return; }
            if (adsInitializing) return;
            adsInitializing = true;
            MobileAds.Initialize(status => Main(() =>
            {
                adsInitializing = false;
                if (this == null || status == null) return;
                if (!COghePrivacy.AdsPermitValid(ticket))
                {
                    if (COghePrivacy.AdsAgeEligible && consentRevision == COghePrivacy.Revision) RequestConsent();
                    return;
                }
                if (!ConsentInformation.CanRequestAds()) return;
                ads = gameObject.AddComponent<COgheAdMob>();
                COgheAds.Provider = ads;
                COgheAds.RefreshBanner();
                Debug.Log("[COghe services] AdMob ready with restricted personalization.");
            }));
        }

        private void Send(string name, IReadOnlyDictionary<string, object> values)
        {
            if (!Ready || !COghePrivacy.AnalyticsAllowed || values == null) return;
            var parameters = new List<Parameter>(values.Count + 4);
            foreach (var pair in values)
            {
                if (pair.Value is bool b) parameters.Add(new Parameter(pair.Key, b ? 1L : 0L));
                else if (pair.Value is int || pair.Value is long) parameters.Add(new Parameter(pair.Key, Convert.ToInt64(pair.Value)));
                else if (pair.Value is float || pair.Value is double) parameters.Add(new Parameter(pair.Key, Convert.ToDouble(pair.Value)));
                else parameters.Add(new Parameter(pair.Key, (pair.Value?.ToString() ?? "").Substring(0, Math.Min(100, pair.Value?.ToString().Length ?? 0))));
            }
            parameters.Add(new Parameter("build_channel", COgheTestTools.LevelSelect ? "test" : "store"));
            parameters.Add(new Parameter("event_schema", 1L));
            parameters.Add(new Parameter("economy_revision", COgheEconomy.Revision));
            parameters.Add(new Parameter("economy_variant", COgheEconomy.Variant));
            FirebaseAnalytics.LogEvent(name, parameters.ToArray());
        }

        private void OnDestroy()
        {
            COghePrivacy.Changed -= ApplyPrivacy;
            CancelInvoke(); ads?.Suspend(); StopAnalytics();
            COgheAnalytics.Sink = null; COgheAds.PrivacyRequired = null; COgheAds.OpenPrivacy = null;
            COgheAds.Provider = new COgheNoAds();
        }
    }
}
