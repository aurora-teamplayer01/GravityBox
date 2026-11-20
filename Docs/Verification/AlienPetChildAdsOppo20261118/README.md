# Alien Pet — OPPO child-ad QA, 18 November 2026

Internal test artifact: 1.0.6-child-qa (7), same package/upload key. Play App Signing must update the existing installation; no uninstall, data clearing, or Unity campaign activation.

Before update: Google Play version 6, level 16, 225 Drops, mint/peach pet. Existing firstInstallTime: 2 November 2026 08:55:28. Evidence in before-menu.png and device-baseline.json.

## Test isolation

The explicitly named Internal AAB includes ALIENPET_CHILD_ADS_QA. Only the explicit alienpet_child_banner_proof launch extra activates the fixture. Its age, progress and shop state are memory-only, save persistence is disabled, and it requests Google test ad IDs. A normal launch uses existing saved state. Production builds exclude this fixture by default; never promote this Internal QA artifact to Production.

The artifact enables Tenjin configuration for gating verification, retaining the minor/adult consent boundary. Repository config is restored to its previous disabled default. No key or device identifier is included in this report.

## Build verification

Bundletool/JAR signature and signed entry digests passed; upload certificate matches the existing key. ARM64 native libraries and bundle use 16 KB alignment. No AD_ID or AdServices permission. Required official Google App Set classes are present per owner authorization. New child banner and existing service IDs are packaged. See validation-report.json.

Native symbols were prepared against the exact AAB Build IDs and uploaded to the release.

## Real OPPO result

Internal release `1.0.6 (7) - Child Banner Internal QA` was published to existing Internal testers at 10:28 on 18 November 2026. OPPO CPH2591 updated through Google Play (`installerPackageName=com.android.vending`) at 10:30:10. Original installation time stayed 2 November 2026 08:55:28; no uninstall or clear-data operation occurred. Production and Unity campaigns were not changed.

Two explicit QA launches completed on the physical phone. Both reports show:

- Native Google test banner loaded; visible Test Ad at the bottom of Menu (`oppo-child-menu.png`).
- Child age treatment and personalization Disabled. Source config sets maximum rating G; the SDK object's default ToString in the fixture prints its type, so the text report alone does not independently prove the rating value.
- AnalyticsAllowed=false, FirebaseReady=false, zero Tenjin transport instances despite Tenjin being enabled in this test artifact.
- RewardedReady=false and InterstitialReady=false.
- Real Victory UI reports banner height 0 and zero child reward-ad offers; visual confirmation in `oppo-child-victory.png` (captured after the fixture also transitions to Under9).
- Under9 becomes ineligible for ads and has zero banner height.
- Both captured process logs contain zero fatal exceptions, missing AppSet failures, AndroidJavaException or matching Tenjin-connect markers. These are targeted log checks, not packet-level proof of zero network activity.

Reports: `device-result.txt`, `device-result-repeat.txt`, `device-log-summary.json`, `device-log-repeat-summary.json`.

After stopping the fixture, launched the app normally without any test extra. `after-menu.png` confirms level 16, 225 Drops and the original mint/peach pet. Fixture age/consent/progress were memory-only; original stored preferences were not overwritten. Game remains open normally for the owner.

## Limits and handoff

These were Google **test ads**: native rendering and protection checks passed; live child-unit fill and revenue are not established by this test. Home/gameplay placement guards are covered by the earlier automated suites, not an additional physical-phone walkthrough in this run. Existing 29 privacy/config tests and 16 UI/reward tests are recorded in ../AlienPetChildAds20261118.

Do not promote version 7 to Production: it contains the explicitly flagged Internal-only fixture. Build a clean production AAB without ALIEN_PET_CHILD_ADS_QA after release checks. No commit/push was requested or performed.
