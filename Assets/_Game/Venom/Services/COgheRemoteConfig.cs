using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Fetch once per launch, with SDK caching/throttling. Stage validated values for the next launch.</summary>
    public static class COgheRemoteConfig
    {
        public static async void Fetch()
        {
            // Remote Config creates a Firebase installation identifier; it shares the adult opt-in gate.
            if (!COghePrivacy.AnalyticsAllowed || !COgheGoogleServices.Ready) return;
            int ticket = COghePrivacy.Revision;
            try
            {
                var config = FirebaseRemoteConfig.DefaultInstance;
                var defaults = COgheEconomy.RemoteDefaults();
                await config.SetDefaultsAsync(defaults);
                if (!COghePrivacy.AnalyticsPermitValid(ticket)) return;
                await config.FetchAsync(TimeSpan.FromHours(1));
                if (!COghePrivacy.AnalyticsPermitValid(ticket)) return;
                if (config.Info.LastFetchStatus != LastFetchStatus.Success)
                { Debug.LogWarning("[COghe config] Fetch unavailable; keeping current defaults/cache."); return; }
                await config.ActivateAsync();
                if (!COghePrivacy.AnalyticsPermitValid(ticket)) return;
                var values = new Dictionary<string, string>();
                foreach (var key in defaults.Keys) values[key] = config.GetValue(key).StringValue;
                bool staged = COgheEconomy.StageRemote(values);
                COgheAnalytics.Log("economy_config_fetch", "staged", staged, "next_revision", values["economy_revision"]);
                Debug.Log("[COghe config] fetched; staged=" + staged + "; next launch revision=" + values["economy_revision"]);
            }
            catch (Exception e) { Debug.LogWarning("[COghe config] Offline/defaults retained: " + e.GetType().Name); }
        }
    }
}
