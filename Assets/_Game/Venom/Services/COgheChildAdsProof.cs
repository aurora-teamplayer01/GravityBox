#if UNITY_ANDROID && (DEVELOPMENT_BUILD || ALIENPET_CHILD_ADS_QA)
using System.Collections;
using System.IO;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Explicit native child-banner QA, memory-only age/progress. Included only in development or explicitly flagged Internal QA builds.</summary>
    public sealed class COgheChildAdsProof : MonoBehaviour
    {
        private static bool requested;
        public static bool Requested => requested;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Configure()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            requested = intent.Call<bool>("getBooleanExtra", "alienpet_child_banner_proof", false);
            if (!requested) return;
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Age9To12);
            VenomCampaignSave.PersistenceEnabled = false;
            COgheProductMode.OverrideForTests = true;
            COgheProductUI.PrivacyGateForTests = true; // Persistence is disabled, but production privacy UI stays enabled.
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true });
            COgheEntitlements.ResetForTests();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartProof()
        {
            if (!requested) return;
            var go = new GameObject("Child banner QA"); DontDestroyOnLoad(go); go.AddComponent<COgheChildAdsProof>();
        }
        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(3);
            var game = FindFirstObjectByType<VenomCampaign>();
            var ui = game.ProductUI;
            ui.ShowMenu();
            float deadline = Time.realtimeSinceStartup + (COgheGoogleServices.AdsDependencyBlocked ? 2 : 90);
            while (COgheAds.BannerHeight <= 0 && Time.realtimeSinceStartup < deadline) yield return null;
            string folder = Path.Combine(Application.persistentDataPath, "ChildBannerProof"); Directory.CreateDirectory(folder);
            bool bannerLoaded = COgheAds.BannerHeight > 0;
            var config = COgheGoogleServices.AdRequestConfiguration();
            string report = $"version={Application.version}\nageTreatment={config.AgeRestrictedTreatment}\nmaxAdContentRating={config.MaxAdContentRating}\npersonalization={config.PublisherPrivacyPersonalizationState}\nadsDependencyBlocked={COgheGoogleServices.AdsDependencyBlocked}\nbannerLoaded={bannerLoaded}\nanalyticsAllowed={COghePrivacy.AnalyticsAllowed}\nfirebaseReady={COgheGoogleServices.Ready}\ntenjinTransportCount={FindObjectsByType<COgheRestrictedTenjin>(FindObjectsSortMode.None).Length}\nrewardedReady={COgheAds.Provider?.RewardedReady}\ninterstitialReady={COgheAds.Provider?.InterstitialReady}\n";
            // Android ScreenCapture prepends persistentDataPath itself.
            ScreenCapture.CaptureScreenshot("ChildBannerProof/child-menu.png");
            yield return new WaitForSecondsRealtime(15);
            ui.ShowVictoryForTests(game.Definition.Id, 1);
            yield return new WaitForSecondsRealtime(2);
            int rewardOffers = 0;
            foreach (var child in ui.GetComponentsInChildren<Transform>()) if (child.name == "Triple drops") rewardOffers++;
            report += $"victoryPage={ui.Page}\nvictoryBannerHeight={COgheAds.BannerHeight}\nchildRewardOfferCount={rewardOffers}\n";
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Under9);
            yield return new WaitForSecondsRealtime(3);
            report += $"under9AdsEligible={COghePrivacy.AdsAgeEligible}\nunder9BannerHeight={COgheAds.BannerHeight}\n";
            File.WriteAllText(Path.Combine(folder, "result.txt"), report);
            Debug.Log("ALIENPET CHILD BANNER PROOF\n" + report);
        }
    }
}
#endif
