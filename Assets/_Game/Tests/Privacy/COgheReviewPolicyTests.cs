using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed class COgheReviewPolicyTests
    {
        private bool persistence;
        private COgheShop.State previous;
        [SetUp] public void Before()
        {
            persistence = VenomCampaignSave.PersistenceEnabled;
            previous = COgheShop.Current;
            VenomCampaignSave.PersistenceEnabled = false;
            COgheShop.ResetForTests(new COgheShop.State());
        }
        [TearDown] public void After()
        { COgheShop.ResetForTests(previous); VenomCampaignSave.PersistenceEnabled = persistence; }

        [Test] public void FirstCompletedLessonPersistsOneOpportunityAcrossReloadAndReplay()
        {
            COgheShop.Understand(COgheBonus.RoundKey(1001, 1));
            Assert.IsFalse(COgheShop.TakeReviewOpportunity(1001), "A correct answer is not the completed lesson.");
            COgheShop.CompleteUnderstandBonus(1001);
            COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(JsonUtility.ToJson(COgheShop.Current)));
            Assert.IsFalse(COgheShop.TakeReviewOpportunity(1), "Only the completed lesson owns this opportunity.");
            Assert.IsTrue(COgheShop.TakeReviewOpportunity(1001));
            COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(JsonUtility.ToJson(COgheShop.Current)));
            COgheShop.CompleteUnderstandBonus(1001);
            COgheShop.CompleteUnderstandBonus(1);
            Assert.IsFalse(COgheShop.TakeReviewOpportunity(1001));
            Assert.IsFalse(COgheShop.TakeReviewOpportunity(1));
            Assert.AreEqual(0, COgheShop.Drops, "Review bookkeeping never grants a reward.");
        }

        [TestCase(true)] [TestCase(false)]
        public void ExistingCompletedLessonsDoNotRetroactivelyPrompt(bool legacyPaid)
        {
            if (legacyPaid) COgheShop.Current.Paid.Add(COgheBonus.Key(1));
            else COgheShop.Understand(COgheBonus.Key(1001));
            COgheShop.CompleteUnderstandBonus(1002);
            Assert.AreEqual(0, COgheShop.Current.ReviewBonus);
            Assert.IsFalse(COgheShop.TakeReviewOpportunity(1002));
        }

        [Test] public void SkippedPracticeIsNotACompletedLessonAndInvalidIdsNeverPrompt()
        {
            COgheShop.Current.SkippedPractices.Add(1001);
            COgheShop.CompleteUnderstandBonus(0);
            Assert.AreEqual(0, COgheShop.Current.ReviewBonus);
            COgheShop.CompleteUnderstandBonus(1);
            Assert.IsTrue(COgheShop.TakeReviewOpportunity(1));
        }
    }
}
