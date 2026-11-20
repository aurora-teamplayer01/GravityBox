using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;

namespace GravityBox.Tests
{
    /// <summary>The App Store bridge's C# side: StoreKit events become prices, purchase results and entitlements.</summary>
    public sealed class COgheAppleStoreTests
    {
        private sealed class FakeBridge : COgheAppleStore.IBridge
        {
            public string StartedWith; public readonly List<string> Purchases = new List<string>(); public int Restores;
            public void Start(string ids, COgheAppleStore receiver) => StartedWith = ids;
            public void Purchase(string product) => Purchases.Add(product);
            public void Restore() => Restores++;
        }

        private FakeBridge bridge; private COgheAppleStore store;
        private const string BothProducts = "{\"ids\":[\"coghe_plus\",\"coghe_no_ads\"],\"prices\":[\"$5.99\",\"$2.99\"]}";

        [SetUp] public void SetUp()
        {
            COgheEntitlements.ResetForTests();
            bridge = new FakeBridge(); store = new COgheAppleStore(bridge);
            COgheEntitlements.Store = store;
        }
        [TearDown] public void TearDown() => COgheEntitlements.ResetForTests();

        [Test] public void StartsWithBothProductsAndStaysHiddenUntilApplePricesBoth()
        {
            Assert.AreEqual("coghe_plus,coghe_no_ads", bridge.StartedWith);
            Assert.IsFalse(store.Available);
            store.Receive(COgheAppleStore.KindProducts, "{\"ids\":[\"coghe_plus\"],\"prices\":[\"$5.99\"]}");
            Assert.IsFalse(store.Available, "One product missing on App Store Connect: no half shop");
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            Assert.IsTrue(COgheEntitlements.StoreAvailable);
            Assert.AreEqual("$5.99", store.Price(COgheEntitlements.Plus));
            Assert.AreEqual("$2.99", store.Price(COgheEntitlements.NoAds));
        }

        [Test] public void OfflineOrErrorKeepsPurchasesUnavailable()
        {
            store.Receive(COgheAppleStore.KindProducts, "{\"ids\":[],\"prices\":[],\"error\":\"offline\"}");
            bool? result = null;
            COgheEntitlements.Buy(COgheEntitlements.Plus, ok => result = ok);
            Assert.AreEqual(false, result);
            Assert.IsEmpty(bridge.Purchases);
        }

        [Test] public void APurchaseGrantsOnlyWhenAppleConfirms()
        {
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            bool? result = null;
            COgheEntitlements.Buy(COgheEntitlements.Plus, ok => result = ok);
            CollectionAssert.AreEqual(new[] { "coghe_plus" }, bridge.Purchases);
            Assert.IsNull(result, "Waits for StoreKit");
            Assert.IsFalse(COgheEntitlements.HasPlus);
            store.Receive(COgheAppleStore.KindPurchase, "{\"id\":\"coghe_plus\",\"ok\":true,\"reason\":\"done\"}");
            Assert.AreEqual(true, result);
            Assert.IsTrue(COgheEntitlements.HasPlus && COgheEntitlements.AdsRemoved);
        }

        [TestCase("cancelled")]
        [TestCase("pending")]
        [TestCase("unverified")]
        public void ACancelledPendingOrUnverifiedPurchaseGrantsNothing(string reason)
        {
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            bool? result = null;
            COgheEntitlements.Buy(COgheEntitlements.NoAds, ok => result = ok);
            store.Receive(COgheAppleStore.KindPurchase, "{\"id\":\"coghe_no_ads\",\"ok\":false,\"reason\":\"" + reason + "\"}");
            Assert.AreEqual(false, result);
            Assert.IsFalse(COgheEntitlements.HasNoAds);
            Assert.AreEqual(reason, store.LastFailure);
        }

        [Test] public void AskToBuyApprovalLaterGrantsThroughTheUpdatesStream()
        {
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            COgheEntitlements.Buy(COgheEntitlements.NoAds, null);
            store.Receive(COgheAppleStore.KindPurchase, "{\"id\":\"coghe_no_ads\",\"ok\":false,\"reason\":\"pending\"}");
            store.Receive(COgheAppleStore.KindGrant, "{\"id\":\"coghe_no_ads\"}");
            Assert.IsTrue(COgheEntitlements.HasNoAds);
            Assert.IsFalse(COgheEntitlements.HasPlus);
        }

        [Test] public void ADoubleTapStartsOnePurchase()
        {
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            bool? second = null;
            COgheEntitlements.Buy(COgheEntitlements.Plus, null);
            COgheEntitlements.Buy(COgheEntitlements.Plus, ok => second = ok);
            Assert.AreEqual(1, bridge.Purchases.Count);
            Assert.AreEqual(false, second);
        }

        [Test] public void RestoreReportsAndGrantsWhatAppleFinds()
        {
            int calls = 0; bool? restored = null;
            COgheEntitlements.Restore(ok => { calls++; restored = ok; });
            Assert.AreEqual(false, restored, "Store not reached yet: nothing to restore, no hang");
            Assert.AreEqual(0, bridge.Restores);
            store.Receive(COgheAppleStore.KindProducts, BothProducts);
            restored = null;
            COgheEntitlements.Restore(ok => { calls++; restored = ok; });
            COgheEntitlements.Restore(ok => calls++);
            Assert.AreEqual(1, bridge.Restores, "One AppStore.sync for both taps");
            Assert.IsNull(restored);
            store.Receive(COgheAppleStore.KindGrant, "{\"id\":\"coghe_plus\"}");
            store.Receive(COgheAppleStore.KindRestore, "{\"ok\":true}");
            Assert.AreEqual(true, restored);
            Assert.AreEqual(3, calls);
            Assert.IsTrue(COgheEntitlements.HasPlus);
        }

        [Test] public void ARefundTakesThePerksAway()
        {
            store.Receive(COgheAppleStore.KindGrant, "{\"id\":\"coghe_plus\"}");
            Assert.AreEqual(2, COgheEntitlements.DropsMultiplier);
            store.Receive(COgheAppleStore.KindRevoke, "{\"id\":\"coghe_plus\"}");
            Assert.IsFalse(COgheEntitlements.HasPlus);
            Assert.AreEqual(1, COgheEntitlements.DropsMultiplier);
        }

        [Test] public void UnknownProductIdsAreIgnored()
        {
            store.Receive(COgheAppleStore.KindGrant, "{\"id\":\"someone_elses_product\"}");
            Assert.IsFalse(COgheEntitlements.HasPlus || COgheEntitlements.HasNoAds);
        }
    }
}
