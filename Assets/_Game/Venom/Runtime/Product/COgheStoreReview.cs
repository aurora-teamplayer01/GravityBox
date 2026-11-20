using System;
using System.Collections;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Native review presentation only. No score, rewards, telemetry or consent changes.</summary>
    public static class COgheStoreReview
    {
        // Android registers the Google provider; other platforms have no automatic store redirect.
        public static Func<IEnumerator> RequestFlow;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { RequestFlow = null; }
    }

    public sealed partial class COgheProductUI
    {
        private bool reviewRunning;
        public bool ReviewRunning => reviewRunning;

        private IEnumerator ReviewBeforeBonusContinuation(int chapter)
        {
            IEnumerator flow = null;
            try
            {
                try { flow = COgheStoreReview.RequestFlow?.Invoke(); }
                catch (Exception) { Debug.LogWarning("[Alien Pet review] Store flow unavailable; continuing."); }
                while (flow != null)
                {
                    bool more;
                    object wait = null;
                    try { more = flow.MoveNext(); if (more) wait = flow.Current; }
                    catch (Exception) { Debug.LogWarning("[Alien Pet review] Store flow failed; continuing."); break; }
                    if (!more) break;
                    yield return wait;
                }
            }
            finally
            {
                (flow as IDisposable)?.Dispose();
                reviewRunning = false;
            }
            // Google deliberately does not report whether a sheet appeared or a rating was submitted.
            // Dismissal, quota suppression and errors all preserve the earned card and normal progression.
            if (this != null && Page == COgheProductPage.Home && bonusChapter == chapter)
                FinishBonusContinuation();
        }
    }
}
