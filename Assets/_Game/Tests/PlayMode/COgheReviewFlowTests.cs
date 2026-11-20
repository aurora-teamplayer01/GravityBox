using System;
using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        [UnityTest, Timeout(120000)] public IEnumerator ReviewAfterFirstUnderstandReturnsToEarnedCardWithoutChangingPrivacy()
        {
            int requests = 0; bool release = false;
            IEnumerator Sheet() { requests++; while (!release) yield return null; }
            COgheStoreReview.RequestFlow = Sheet;
            var age = COghePrivacy.Age; bool analytics = COghePrivacy.AnalyticsAllowed, measurement = COghePrivacy.AdMeasurementAllowed;
            try
            {
                yield return CompleteFirstReviewLesson();
                Assert.AreEqual(0, requests, "Do not cover the completion celebration before Continue.");
                int drops = COgheShop.Drops;
                yield return Click("Bonus continue");
                Assert.AreEqual(1, requests); Assert.IsTrue(ui.ReviewRunning); Assert.IsTrue(ui.BlockWorldInput);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Single(b => b.name == "Bonus continue").interactable);
                ui.Resume(); Assert.AreEqual(COgheProductPopup.BonusDone, ui.Popup, "Back cannot advance under a native sheet.");
                release = true;
                yield return AwaitBonus(() => ui.AbilityChapter == 1001, 5, "Dismissal returns to the earned card");
                Assert.IsFalse(ui.ReviewRunning); Assert.AreEqual(drops, COgheShop.Drops);
                Assert.AreEqual(age, COghePrivacy.Age); Assert.AreEqual(analytics, COghePrivacy.AnalyticsAllowed);
                Assert.AreEqual(measurement, COghePrivacy.AdMeasurementAllowed);
                yield return Click("Emotion card"); yield return Click("Choose ability"); yield return Click("Ability continue");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Emotion));
                Assert.AreEqual(COgheProductPopup.None, ui.Popup);
                Assert.IsFalse(COgheShop.TakeReviewOpportunity(1001)); Assert.AreEqual(1, requests);
            }
            finally { release = true; COgheStoreReview.RequestFlow = null; }
        }

        [UnityTest, Timeout(120000)] public IEnumerator ReviewProviderFailureNeverLosesTheAbilityCard()
        {
            COgheStoreReview.RequestFlow = () => throw new InvalidOperationException("test unavailable store");
            try
            {
                yield return CompleteFirstReviewLesson();
                int drops = COgheShop.Drops;
                yield return Click("Bonus continue");
                Assert.AreEqual(1001, ui.AbilityChapter); Assert.IsFalse(ui.ReviewRunning);
                Assert.AreEqual(drops, COgheShop.Drops); Assert.AreEqual(-1, COgheShop.Current.ReviewBonus);
            }
            finally { COgheStoreReview.RequestFlow = null; }
        }

        [UnityTest, Timeout(120000)] public IEnumerator ReviewSuppressedByStoreContinuesWithoutASecondPrompt()
        {
            int requests = 0;
            IEnumerator Suppressed() { requests++; yield break; }
            COgheStoreReview.RequestFlow = Suppressed;
            try
            {
                yield return CompleteFirstReviewLesson(); yield return Click("Bonus continue");
                Assert.AreEqual(1, requests); Assert.AreEqual(1001, ui.AbilityChapter);
                Assert.IsFalse(ui.ReviewRunning); Assert.IsFalse(COgheShop.TakeReviewOpportunity(1001));
            }
            finally { COgheStoreReview.RequestFlow = null; }
        }

        private IEnumerator CompleteFirstReviewLesson()
        {
            game.Progress.Completed.Clear(); game.Progress.HomeUnlocked = false;
            foreach (var level in ui.Catalog.Levels.Take(6)) game.Progress.Win(level);
            ui.StartBonus(1001, false); yield return null;
            yield return PlayUnderstand(game.Personality, "First review lesson");
            yield return AwaitBonus(() => ui.Popup == COgheProductPopup.BonusDone, 30, "Completed first lesson");
            Assert.IsTrue(COgheBonus.Done(1001));
        }
    }
}
