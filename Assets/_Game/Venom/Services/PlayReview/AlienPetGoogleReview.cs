using System.Collections;
using Google.Play.Review;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Official Google sheet, unchanged. Separate from ads, analytics and attribution SDKs.</summary>
    public static class AlienPetGoogleReview
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            COgheStoreReview.RequestFlow = Run;
#endif
        }

        private static IEnumerator Run()
        {
            var manager = new ReviewManager();
            var request = manager.RequestReviewFlow();
            float until = Time.realtimeSinceStartup + 8f;
            while (!request.IsDone)
            {
                // No late launch after the player has continued if Play is offline/unavailable.
                if (Time.realtimeSinceStartup >= until)
                { Debug.Log("[Alien Pet review] Request timed out; continuing."); yield break; }
                yield return null;
            }
            if (request.Error != ReviewErrorCode.NoError)
            { Debug.Log("[Alien Pet review] Request unavailable: " + request.Error); yield break; }
            var info = request.GetResult();
            Debug.Log("[Alien Pet review] Launching native flow.");
            var launch = manager.LaunchReviewFlow(info);
            info = null;
            // Never dismiss or cover Google's sheet; wait for the platform to return control.
            while (!launch.IsDone) yield return null;
            Debug.Log("[Alien Pet review] Flow returned: " + launch.Error + "; rating outcome is not exposed.");
        }
    }
}
