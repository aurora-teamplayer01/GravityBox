# Alien Pet iOS in-app purchases (StoreKit 2)

Decision 16/11/2026: the first iOS release ships **real** purchases, not a no-commerce build. Team: **Bui Ngoc Quang (RPLRU9Q87A)**.

## Products

Created 16/11/2026 through the App Store Connect API in app `6835357287` (bundle ID resource `VL8F9884YX`, IAP capability on): `coghe_plus` = `6835357705`, `coghe_no_ads` = `6835357757`, with USD base price, en-US localization and every territory. **Still missing: one review screenshot per product** (the Plus popup), then submit them with version 1.0.0.

| Product ID | Type | Reference name | Display name (en-US) | Price |
|---|---|---|---|---|
| `coghe_plus` | Non-Consumable | Alien Pet Plus | Alien Pet Plus | $5.99 (tier matching USD 5.99) |
| `coghe_no_ads` | Non-Consumable | No Ads | No Ads | $2.99 |

Store descriptions (Apple limit 55 characters): Plus — "No ads, rewards without ads, double Drops". No Ads — "Removes banner and full-screen ads". Each product needs a review screenshot (the Plus popup in the game) and must be submitted **with the first app version**. Family Sharing: off (matches the code; turning it on is fine, StoreKit then grants to family members automatically).

Requirements on the account: Paid Applications Agreement signed, tax and banking forms complete. Without them StoreKit returns no products and the game hides every purchase button ("Purchases are not available yet.").

## How the code works

- `Assets/Plugins/iOS/AlienPetStoreKit.swift`: StoreKit 2 (`Product.products`, `purchase()`, `Transaction.updates`, `Transaction.currentEntitlements`, `AppStore.sync`). Only verified, unrevoked transactions grant; refunds revoke.
- `Assets/_Game/Venom/Runtime/Shop/COgheAppleStore.cs`: `ICOgheStore` for iOS store builds. Starts at launch, so purchases come back after a reinstall or on another device, and Ask to Buy approvals land later.
- The shop shows purchases only when Apple returned **both** products with prices. Prices are Apple's localized `displayPrice`.
- Development and test builds (`COGHE_TEST_TOOLS`) keep the free test store; a `COGHE_STORE=1` build never contains it.
- No receipt server: entitlements are on-device flags re-checked from StoreKit at every launch.
- Tests: `Assets/_Game/Tests/Privacy/COgheAppleStoreTests.cs` (EditMode, fake bridge).

## Local testing without App Store Connect

`technical/AlienPet.storekit` is a StoreKit configuration with both products. In the exported Xcode project: Product → Scheme → Edit Scheme → Run → Options → StoreKit Configuration → choose this file, then run on a device or simulator. Purchases are free and can be refunded or deleted in Debug → StoreKit → Manage Transactions.

## Device QA (TestFlight, sandbox account)

1. Fresh install, open the Plus popup: both prices shown in the device's currency.
2. Buy No Ads → ads disappear, Plus still offered. Buy Plus → popup shows "Owned", Drops ×2, rewards without ads.
3. Cancel a purchase → nothing granted, no error loop.
4. Delete the app, reinstall, wait at the menu → purchases return without tapping Restore. Tap Restore → "Purchases restored."
5. Ask to Buy (child sandbox account): "pending" grants nothing; approval later grants without restarting.
6. Refund in sandbox (Settings → Developer / Manage Transactions) → perks removed on next launch.
7. Airplane mode at launch → no purchase buttons, game plays normally.
