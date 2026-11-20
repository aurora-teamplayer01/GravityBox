using System;
using System.Collections.Generic;
using GravityBox.Services;
using GravityBox.Venom;
using NUnit.Framework;

namespace GravityBox.Tests
{
    public sealed class COgheAttributionTests
    {
        private sealed class Fake : ICOgheAttributionTransport
        {
            public int connects, stops;
            public bool fail;
            public readonly List<string> events = new List<string>();
            public void Connect() { connects++; if (fail) throw new InvalidOperationException(); }
            public void OptOut() => stops++;
            public void Event(string name, string value) => events.Add(name + ":" + value);
        }
        private Fake fake;
        private COgheAttributionSession session;
        private int creates;
        private readonly Dictionary<string, object> level = new Dictionary<string, object> { { "level", 25 } };
        [SetUp] public void SetUp()
        {
            COghePrivacy.ResetForTests(); COghePrivacy.AdMeasurementFeatureForTests = true; fake = new Fake(); creates = 0;
            session = new COgheAttributionSession(() => { creates++; return fake; });
        }
        [TearDown] public void TearDown() { session.Dispose(); COghePrivacy.ClearTestOverride(); }

        [TestCase(COghePrivacy.AgeBand.Unknown)]
        [TestCase(COghePrivacy.AgeBand.Under13)]
        [TestCase(COghePrivacy.AgeBand.Under9)]
        [TestCase(COghePrivacy.AgeBand.Age9To12)]
        [TestCase(COghePrivacy.AgeBand.Age13To15)]
        [TestCase(COghePrivacy.AgeBand.Age16To17)]
        public void NoSdkObjectOrNetworkForUnknownAgeAndMinors(COghePrivacy.AgeBand age)
        {
            COghePrivacy.ResetForTests(age, true);
            session.ApplyPrivacy(); session.Foreground(); session.Send("level_complete", level);
            Assert.AreEqual(0, creates); Assert.IsFalse(session.Active);
        }
        [Test] public void AdultMustOptInAndOldEventsAreNeverReplayed()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, false);
            session.ApplyPrivacy(); session.Send("level_complete", level);
            Assert.AreEqual(0, creates);
            COghePrivacy.SetAnalyticsChoice(true); session.ApplyPrivacy(); Assert.AreEqual(0, creates);
            COghePrivacy.SetAdMeasurementChoice(true); session.ApplyPrivacy(); session.ApplyPrivacy();
            Assert.AreEqual(1, creates); Assert.AreEqual(1, fake.connects); Assert.IsEmpty(fake.events);
            session.Send("level_complete", level);
            Assert.AreEqual("level_complete:25", fake.events[0]);
            session.Foreground(); Assert.AreEqual(2, fake.connects);
        }
        [Test] public void RevokeThenRegrantInvalidatesOldPermitAndStopsNativeSdk()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true); COghePrivacy.SetAdMeasurementChoice(true); session.ApplyPrivacy();
            COghePrivacy.SetAdMeasurementChoice(false);
            session.Send("level_complete", level); session.ApplyPrivacy(); session.Foreground();
            Assert.AreEqual(1, fake.stops); Assert.AreEqual(1, fake.connects); Assert.IsEmpty(fake.events);
            COghePrivacy.SetAdMeasurementChoice(true);
            session.Send("level_complete", level); Assert.IsEmpty(fake.events);
            session.ApplyPrivacy(); Assert.AreEqual(2, fake.connects); Assert.AreEqual(1, creates);
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Under13); session.ApplyPrivacy();
            Assert.AreEqual(2, fake.stops); Assert.IsFalse(session.Active);
        }
        [Test] public void OnlyBoundedLevelEventsLeaveTheGame()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true); COghePrivacy.SetAdMeasurementChoice(true); session.ApplyPrivacy();
            session.Send("age", level); session.Send("ad_revenue", level); session.Send("iap_purchase", level);
            session.Send("level_end", level); session.Send("level_complete", null);
            session.Send("level_complete", new Dictionary<string, object> { { "level", "email@example.com" } });
            session.Send("level_complete", new Dictionary<string, object> { { "level", -1 } });
            Assert.IsEmpty(fake.events);
            session.Send("level_start", level); session.Send("level_fail", level);
            Assert.AreEqual(2, fake.events.Count);
        }
        [Test] public void FailedConnectOptsOutAndCanRecover()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true); COghePrivacy.SetAdMeasurementChoice(true); fake.fail = true;
            Assert.Throws<InvalidOperationException>(() => session.ApplyPrivacy());
            Assert.IsFalse(session.Active); Assert.AreEqual(1, fake.stops);
            fake.fail = false; session.Foreground(); Assert.IsTrue(session.Active);
        }
        [Test] public void LegacyOptInRequiresNewDisclosureWithoutLosingAge()
        {
            COghePrivacy.LoadChoiceForTests("{\"version\":1,\"age\":4,\"analyticsChoiceMade\":true,\"analytics\":true}");
            Assert.AreEqual(COghePrivacy.AgeBand.Adult, COghePrivacy.Age);
            Assert.IsFalse(COghePrivacy.SetupComplete); Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            session.ApplyPrivacy(); Assert.AreEqual(0, creates);
            COghePrivacy.SetAnalyticsChoice(true); session.ApplyPrivacy(); Assert.IsFalse(session.Active);
            COghePrivacy.SetAdMeasurementChoice(true); session.ApplyPrivacy(); Assert.IsTrue(session.Active);
        }
        [Test] public void LegacyAnalyticsOptInDoesNotGrantAdvertisingIdentifiers()
        {
            COghePrivacy.LoadChoiceForTests("{\"version\":2,\"age\":4,\"analyticsChoiceMade\":true,\"analytics\":true,\"adMeasurement\":true}");
            Assert.IsTrue(COghePrivacy.AnalyticsAllowed);
            Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
            Assert.IsFalse(COghePrivacy.AdMeasurementChoiceMade);
            session.ApplyPrivacy(); Assert.AreEqual(0, creates);
        }
        [Test] public void InstallConsentDoesNotGrantGameplayAnalytics()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, false);
            COghePrivacy.SetAdMeasurementChoice(true); session.ApplyPrivacy();
            Assert.AreEqual(1, fake.connects); Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            session.Send("level_complete", level); Assert.IsEmpty(fake.events);
            COghePrivacy.SetAnalyticsChoice(true); session.ApplyPrivacy(); session.Send("level_complete", level);
            Assert.AreEqual(1, fake.events.Count);
            COghePrivacy.SetAnalyticsChoice(false); session.ApplyPrivacy(); session.Send("level_complete", level);
            Assert.AreEqual(1, fake.events.Count); Assert.IsTrue(session.Active);
        }
        [Test] public void MinorCannotGrantAndAgeChangesResetMeasurementConsent()
        {
            foreach (COghePrivacy.AgeBand age in Enum.GetValues(typeof(COghePrivacy.AgeBand)))
            {
                if (age == COghePrivacy.AgeBand.Adult) continue;
                COghePrivacy.ResetForTests(age, true); COghePrivacy.SetAdMeasurementChoice(true);
                Assert.IsFalse(COghePrivacy.AdMeasurementAllowed); Assert.IsFalse(COghePrivacy.AdMeasurementChoiceMade);
            }
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true);
            COghePrivacy.SetAdMeasurementChoice(true); int ticket = COghePrivacy.Revision;
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Age9To12);
            Assert.IsFalse(COghePrivacy.AdMeasurementPermitValid(ticket));
            COghePrivacy.SetAgeBand(COghePrivacy.AgeBand.Adult); COghePrivacy.SetAnalyticsChoice(true);
            Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
        }
        [Test] public void ProductionFeatureSwitchBlocksEvenSavedConsent()
        {
            COghePrivacy.ResetForTests(COghePrivacy.AgeBand.Adult, true);
            COghePrivacy.SetAdMeasurementChoice(true);
            COghePrivacy.AdMeasurementFeatureForTests = false;
            session.ApplyPrivacy(); Assert.AreEqual(0, creates); Assert.IsFalse(COghePrivacy.AdMeasurementAllowed);
        }
        [Test] public void MigrationPreservesPriorRefusalAndMinorProtection()
        {
            COghePrivacy.LoadChoiceForTests("{\"version\":1,\"age\":4,\"analyticsChoiceMade\":true,\"analytics\":false}");
            Assert.IsTrue(COghePrivacy.SetupComplete); Assert.IsFalse(COghePrivacy.AnalyticsAllowed);
            COghePrivacy.LoadChoiceForTests("{\"version\":1,\"age\":1,\"analyticsChoiceMade\":true,\"analytics\":true}");
            Assert.IsFalse(COghePrivacy.AnalyticsAllowed); Assert.IsFalse(COghePrivacy.AdsAgeEligible);
        }
    }
}
