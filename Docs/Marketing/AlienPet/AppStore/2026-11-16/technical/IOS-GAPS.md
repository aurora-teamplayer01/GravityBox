# iOS implementation status — branch IOS, 16 November 2026

This supersedes the initial documentation-only audit at `dec14e929897b4268421553ac51ab02abd297d09`. Implementation starts from NewGraphic `b8b69f03`. Full configuration: [IOS-SERVICES.md](IOS-SERVICES.md).

| Area | Implemented / verified | Remaining acceptance gate |
|---|---|---|
| Service lifecycle | Android + iOS native Boot, correct bundle, platform-specific production/test ad units | iPhone/iPad native privacy/network behavior across fresh install and age changes |
| AdMob | Real iOS app + 3 units; Firebase linked; separate EU/US UMP messages published | Attach App Store listing, readiness/app-ads.txt, real permitted serving |
| Firebase | Real iOS app + stream; matching GoogleService-Info.plist in source and native app | Adult opt-in DebugView level_complete/level_fail and permitted ad events on device |
| Child protection | Existing unknown/under13 off, teens restricted/no gameplay Analytics, adult explicit opt-in preserved; native collection defaults closed | Native age matrix, consent refusal/withdrawal, verify no unauthorized event delivery |
| Export | Store wrapper with explicit marketing/build versions, bundle validation, 65 main levels + 2 catalog prototypes | Apple distribution signing, archive validation, TestFlight |
| Native dependencies | Release ARM64 unsigned native build succeeded; Firebase Swift resource bundles are embedded by the app-target build phase | Xcode privacy report, required-reason API review and final App Privacy declarations |
| App icon | iOS slots now use approved C source, replacing stale iOS-only icon | Inspect final signed archive icon |
| Device helper | Finds app by bundle ID, no obsolete COghe.app basename | Actual receiving-machine install/signing |
| Simulator | Spatial simulator export option added; generic prototype helper is not the game workflow | Simulator execution not tested in this task |
| IAP | Owner chose real commerce (16/11). StoreKit 2 provider for `coghe_plus` / `coghe_no_ads`: purchase, restore, launch re-grant, Ask to Buy, refund revoke; EditMode tests; local `.storekit` config. See [IOS-IAP.md](IOS-IAP.md) | Create both products + Paid Apps Agreement in App Store Connect; sandbox/TestFlight purchase, restore, reinstall and refund on device |
| Screenshots | Approved icon + 10 draft layouts and capture tooling | Replace Android/Unity sources with actual final iOS captures |
| Policy/support | Existing URL and contact retained | Update policy for actual iOS SDK use; verify public Safari access (automated fetch previously 403) |

## Evidence boundaries

12 EditMode privacy/config tests and 8 PlayMode monetization tests passed. Native Release build compiled and linked on this Mac without signing. These do not establish iOS ad fill, event delivery, legitimate production revenue, purchase correctness or App Review approval. Device QA rows remain NOT_RUN until actual evidence is recorded. No private signing key, provisioning profile or Apple API credential is included.
