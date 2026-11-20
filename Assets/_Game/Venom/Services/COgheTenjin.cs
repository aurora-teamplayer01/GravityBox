using System;
using System.Collections.Generic;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Android attribution only. Never attach TenjinObject: its automatic initialization bypasses consent.</summary>
    public sealed class COgheTenjin : MonoBehaviour
    {
        [Serializable] private sealed class Settings { public bool androidEnabled; public string androidSdkKey; }
        private COgheAttributionSession session;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.identifier != COgheGoogleServices.AppBundleId || Application.isBatchMode) return;
#if ALIENPET_AD_MEASUREMENT_QA
            // Do not silently expose identifiers to another SDK through app-wide AD_ID.
            // This build is solely for attribution validation, not ad-serving validation.
            COgheAds.TestAds = false;
            COgheAds.Provider = new COgheNoAds();
#endif
            var config = Resources.Load<TextAsset>("AlienPetTenjin");
            if (config == null) return;
            var settings = JsonUtility.FromJson<Settings>(config.text);
            if (settings == null || string.IsNullOrWhiteSpace(settings.androidSdkKey)) return;
#if !ALIENPET_AD_MEASUREMENT_QA
            if (!settings.androidEnabled || !COghePrivacy.AdMeasurementFeatureEnabled) return;
#endif
            var host = new GameObject("Alien Pet attribution");
            DontDestroyOnLoad(host);
            host.AddComponent<COgheTenjin>().Initialize(settings.androidSdkKey);
#endif
        }

        private void Initialize(string sdkKey)
        {
            session = new COgheAttributionSession(() => new AndroidTransport(gameObject, sdkKey));
            COghePrivacy.Changed += ApplyPrivacy;
            COgheAnalytics.Observed += Send;
            ApplyPrivacy();
        }

        private void ApplyPrivacy() => Safely(() => session?.ApplyPrivacy());
        private void Send(string name, IReadOnlyDictionary<string, object> values) => Safely(() => session?.Send(name, values));
        private void OnApplicationPause(bool paused) { if (!paused) Safely(() => session?.Foreground()); }
        private void OnDestroy()
        {
            COghePrivacy.Changed -= ApplyPrivacy;
            COgheAnalytics.Observed -= Send;
            Safely(() => session?.Dispose());
        }
        private static void Safely(Action action)
        {
            try { action(); }
            catch (Exception e) { Debug.LogWarning("[Alien Pet attribution] Unavailable: " + e.GetType().Name); }
        }

        private sealed class AndroidTransport : ICOgheAttributionTransport
        {
            private readonly COgheRestrictedTenjin sdk;
            public AndroidTransport(GameObject host, string key)
            {
                sdk = host.AddComponent<COgheRestrictedTenjin>();
                sdk.Init(key);
                sdk.SetCacheEventSetting(false);
                sdk.SetAppStoreType(AppStoreType.googleplay);
                sdk.OptOutGoogleDMA();
            }
            public void Connect() => sdk.Connect();
            public void OptOut() => sdk.OptOut();
            public void Event(string name, string integerValue) => sdk.SendEvent(name, integerValue);
        }
    }
}
