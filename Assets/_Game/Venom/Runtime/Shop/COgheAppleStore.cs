using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using AOT;
#endif

namespace GravityBox.Venom
{
    /// <summary>
    /// App Store purchases through StoreKit 2 (Assets/Plugins/iOS/AlienPetStoreKit.swift). Only an iOS player build talks
    /// to Apple; elsewhere <see cref="ForPlatform"/> returns null and the shop stays "not available". The native side sends
    /// (kind, JSON) events that <see cref="Receive"/> turns into prices, results and entitlements, so the logic is testable
    /// in the editor with a fake bridge.
    /// </summary>
    public sealed class COgheAppleStore : ICOgheStore
    {
        public const int KindProducts = 1, KindPurchase = 2, KindRestore = 3, KindGrant = 4, KindRevoke = 5;
        public static readonly string[] Products = { COgheEntitlements.Plus, COgheEntitlements.NoAds };

        /// <summary>The calls into the native store (StoreKit on a device, a fake in tests).</summary>
        public interface IBridge
        {
            void Start(string commaIds, COgheAppleStore receiver);
            void Purchase(string product);
            void Restore();
        }

        [Serializable] private class ProductsMsg { public string[] ids; public string[] prices; public string error; }
        [Serializable] private class PurchaseMsg { public string id; public bool ok; public string reason; }
        [Serializable] private class RestoreMsg { public bool ok; }
        [Serializable] private class IdMsg { public string id; }

        private readonly IBridge bridge;
        private readonly Dictionary<string, string> prices = new Dictionary<string, string>();
        private readonly Dictionary<string, Action<bool>> purchases = new Dictionary<string, Action<bool>>();
        private readonly List<Action<bool>> restores = new List<Action<bool>>();

        /// <summary>The reason StoreKit gave for the last purchase that did not complete (cancelled, pending, ...).</summary>
        public string LastFailure { get; private set; }

        public COgheAppleStore(IBridge bridge)
        {
            this.bridge = bridge;
            bridge.Start(string.Join(",", Products), this);
        }

        /// <summary>A store build starts StoreKit at launch: owned purchases come back from the receipt (also after a
        /// reinstall or on another device) and Ask to Buy approvals land even before the shop is opened.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!Application.isBatchMode) _ = COgheEntitlements.Store;
#endif
        }

        /// <summary>The live App Store on an iOS device; null in the editor and on other platforms.</summary>
        public static ICOgheStore ForPlatform()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new COgheAppleStore(new NativeBridge());
#else
            return null;
#endif
        }

        /// <summary>Available once Apple returned every product with its price (offline or misconfigured: hidden).</summary>
        public bool Available { get { foreach (var p in Products) if (!prices.ContainsKey(p)) return false; return true; } }
        public string Price(string product) => prices.TryGetValue(product, out var p) ? p : "";

        public void Purchase(string product, Action<bool> done)
        {
            if (!Available || purchases.ContainsKey(product)) { done?.Invoke(false); return; }
            purchases[product] = done;
            bridge.Purchase(product);
        }

        public void Restore(Action<bool> done)
        {
            restores.Add(done);
            if (restores.Count == 1) bridge.Restore();
        }

        /// <summary>One event from the native store (main thread).</summary>
        public void Receive(int kind, string json)
        {
            switch (kind)
            {
                case KindProducts:
                    var products = JsonUtility.FromJson<ProductsMsg>(json);
                    if (!string.IsNullOrEmpty(products.error)) Debug.LogWarning("App Store products: " + products.error);
                    for (int i = 0; products.ids != null && i < products.ids.Length && i < products.prices.Length; i++)
                        prices[products.ids[i]] = products.prices[i];
                    COgheEntitlements.NotifyChanged();
                    break;
                case KindPurchase:
                    var result = JsonUtility.FromJson<PurchaseMsg>(json);
                    if (!result.ok) LastFailure = result.reason;
                    if (purchases.TryGetValue(result.id ?? "", out var done)) { purchases.Remove(result.id); done?.Invoke(result.ok); }
                    break;
                case KindRestore:
                    bool ok = JsonUtility.FromJson<RestoreMsg>(json).ok;
                    var waiting = restores.ToArray(); restores.Clear();
                    foreach (var r in waiting) r?.Invoke(ok);
                    break;
                case KindGrant:
                    COgheEntitlements.Grant(JsonUtility.FromJson<IdMsg>(json).id);
                    break;
                case KindRevoke:
                    COgheEntitlements.Revoke(JsonUtility.FromJson<IdMsg>(json).id);
                    break;
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        private sealed class NativeBridge : IBridge
        {
            private delegate void Callback(int kind, IntPtr json);
            [DllImport("__Internal")] private static extern void AlienPetStore_Start(string ids, Callback callback);
            [DllImport("__Internal")] private static extern void AlienPetStore_Purchase(string id);
            [DllImport("__Internal")] private static extern void AlienPetStore_Restore();

            private static COgheAppleStore receiver;

            [MonoPInvokeCallback(typeof(Callback))]
            private static void OnEvent(int kind, IntPtr json)
            {
                string text = Marshal.PtrToStringUTF8(json);
                try { receiver?.Receive(kind, text); }
                catch (Exception e) { Debug.LogException(e); }
            }

            public void Start(string commaIds, COgheAppleStore r) { receiver = r; AlienPetStore_Start(commaIds, OnEvent); }
            public void Purchase(string product) => AlienPetStore_Purchase(product);
            public void Restore() => AlienPetStore_Restore();
        }
#endif
    }
}
