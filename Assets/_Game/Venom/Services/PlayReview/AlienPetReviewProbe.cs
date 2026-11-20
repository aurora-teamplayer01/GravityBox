#if UNITY_ANDROID && ALIENPET_REVIEW_QA && !UNITY_EDITOR
using System.Collections;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Explicit Internal-test intent only. Invokes the real SDK without editing the player's save.</summary>
    public sealed class AlienPetReviewProbe : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            if (!intent.Call<bool>("getBooleanExtra", "alienpet_review_probe", false)) return;
            intent.Call("removeExtra", "alienpet_review_probe");
            var host = new GameObject("Internal review probe");
            DontDestroyOnLoad(host); host.AddComponent<AlienPetReviewProbe>();
        }
        private IEnumerator Start()
        {
            float until = Time.realtimeSinceStartup + 45;
            COgheProductUI ui = null;
            while (Time.realtimeSinceStartup < until)
            {
                ui = FindFirstObjectByType<COgheProductUI>();
                if (ui != null && ui.Page == COgheProductPage.MainMenu) break;
                yield return null;
            }
            if (ui != null && ui.Page == COgheProductPage.MainMenu && COgheStoreReview.RequestFlow != null)
            {
                Debug.Log("[Alien Pet review QA] Native probe started; player save untouched.");
                yield return COgheStoreReview.RequestFlow();
                Debug.Log("[Alien Pet review QA] Native probe returned; no rating submitted by automation.");
            }
            Destroy(gameObject);
        }
    }
}
#endif
