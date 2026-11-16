# iOS source audit and implementation backlog

Audited source: `dec14e929897b4268421553ac51ab02abd297d09` / NewGraphic, 16 November 2026. Static inspection only; no new release binary or device test in this handoff.

| Priority | Observed source fact | Required work and acceptance |
|---|---|---|
| P0 | `Assets/_Game/Venom/Services/COgheGoogleServices.cs` Boot is inside `UNITY_ANDROID && !UNITY_EDITOR` | Add a platform-aware iOS service lifecycle, with separate IDs/config; do not simply remove guard while keeping Android constants. Verify native startup honors age/consent before Firebase/ads initialize. |
| P0 | `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` has empty `adMobIOSAppId` | Create the real iOS app/units, UMP and app configuration. Test rewarded grant exactly once, close-without-reward, load/show error, interstitial placement and ad paid events. Test revenue is not production income. |
| P0 | No tracked GoogleService-Info.plist found in Assets | Register correct Firebase iOS app; add matching config deliberately and verify plist included in archive. Android config/stream is not equivalent. |
| P0 | No StoreKit/Unity Purchasing provider found in scoped project search; `Packages/manifest.json` has no Unity Purchasing dependency | Confirm intended commerce provider or implement it. `coghe_plus`/`coghe_no_ads` are local entitlement keys, not proof of App Store products. Product/UI must never show simulated purchase success in release. |
| P0 | No tracked PrivacyInfo.xcprivacy found in Assets during audit | Inspect resolved Pods/generated archive too; absence in Assets does not prove absence in package. Audit app-owned required-reason APIs and all SDK manifests/signatures with Xcode validation. Declare actual collected data/tracking. |
| P1 | `COgheIOSBuilder` writes approved name/identifier temporarily, but ProjectSettings remains prototype ID, version 0.1.0/build 0 | Use Spatial builder, explicitly set intended marketing version and build; inspect exported Info.plist. Avoid generic Tools/build.sh path. |
| P1 | `COgheIOSBuilder.ConfigureSigning` is a development-install helper | Unset team/profile/device env during export; configure distribution archive on receiving Mac. Do not treat Release configuration as distribution signing. |
| P1 | `Tools/build-venom-ios.sh` install path ends in `COghe.app` | Locate actual `.app` by CFBundleIdentifier, not a stale file basename. This is test-install convenience, not the archive upload path. |
| P1 | `Tools/run-ios-simulator.sh` builds prototype bundle `com.gravityboxlab.prototype` | Add/use Spatial simulator export preserving SpatialScenePaths and iOS services settings. Do not capture prototype screenshots for Alien Pet. |
| P1 | GooglePlay screenshots are Android/emulator or Unity reference images | Replace with iOS images of final binary; full iPad layout must be captured separately. Renderer final mode rejects unverified source rows. |
| P1 | Current AppIconBuilder explicitly sets default/Android slots | Verify iOS generated asset catalog and 1024 icon in archive; use supplied approved icon if needed. |

## Child and consent invariants

Source of truth: `Assets/_Game/Venom/Runtime/Shop/COghePrivacy.cs` and Android service implementation. Unknown/under13: no ads or Analytics. Teens 13–17: no gameplay Analytics, only appropriately restricted ads after permitted consent. Adults: explicit Analytics choice; withdrawal persists. No stage of iOS initialization, ad adapter, Firebase Remote Config or background startup may bypass this. Test transitions with fresh installs and upgrades. SDK network behavior must be checked on the iOS binary, not inferred from C# alone.

## Scope of this package

No runtime code was changed. These are engineering tasks for the iOS release branch, with acceptance gates in QA-CHECKLIST.csv. Apple account access alone does not resolve them. Do not upload a USB playtest build just because it launches.
