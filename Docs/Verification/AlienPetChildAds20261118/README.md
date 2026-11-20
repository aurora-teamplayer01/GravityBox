# Alien Pet — Android child-banner implementation

Date: 18 November 2026. Work is on the existing IOS checkout, with unrelated pending Tenjin work preserved. This does not deploy a new app release or activate a Unity campaign.

## Runtime contract

- Serialized age enum values 0–4 remain unchanged; Under9=5 and Age9To12=6 are appended. Legacy Under13 remains blocked and is refined only through explicit child-band selection; it never silently gains ads.
- Android 9–12: child-directed, non-personalized **banner only**, Menu/Home only. No victory/gameplay/intro/Style banner; no rewarded, interstitial or app-open ads. iOS remains blocked for child ads pending a separate rollout.
- Unknown/under9/unresolved Under13: no advertising SDK eligibility. Under18: Firebase Analytics/Remote Config/Tenjin remain off; no child ad revenue forwarding to Analytics.
- Configure Child age treatment, maximum G and publisher personalization disabled before SDK init. Disable publisher first-party ID immediately in the init callback, before any ad provider/request: Android SDK 25.4.0 throws if its optional ID setter runs before initialization (found during native QA; Editor simulation does not catch this). UMP receives under-age consent tag for child and teen. No UMP error bypass.
- Generation/revision checks invalidate old load/reward callbacks on age/privacy changes; child eligibility is enforced at both facade and native provider.
- Adult rewards retain earned-callback persistence, deduplication and Plus behavior.
- Native defaults still remove AD_ID/AdServices permissions and auto-init providers. **Owner-approved change during native QA:** restore the official Google App Set dependency required by AdMob 25.4.0; do not substitute fake ID APIs. Tenjin still excludes developer_device_id/scope from uploads and never initializes for minors. Google may process App Set ID; do not claim this build is identifier-free. The runtime fails closed rather than crashing if the required class is missing.

## AdMob configuration

Created Android ad unit `AlienPet_Android_Child9to12_Banner`:
`ca-app-pub-7806638519709442/6851048953`.

Video ad type disabled; text/image/rich media enabled. Custom refresh 120s. Google optimized floor/all prices. No mediation groups/campaigns attached. Child treatment/content filtering are enforced by the app on each request; the ad-unit name itself provides no protection. Development builds use Google's test banner ID, never this unit for QA impressions.

SDK: Google Mobile Ads Unity 11.5.0, resolved Android `play-services-ads:25.4.0`. Google's Families list currently includes AdMob Android >=19.0.0; recheck SDK Index and policy status before shipping.

## Verification

- Final age/config/attribution suite: **29/29 passed** (`privacy-final.xml`).
- Final UI/rewards suite: **16/16 passed** (`play-final.xml`), including legacy Under13 refinement, child no-video offers, adult opt-in/out, phone/tablet age layout, reward deduplication and late reward rejection after age change.
- Test totals are recorded in `test-summary.json`; native results below are separate from Editor assertions.
- Native `COgheChildAdsProof` is DEVELOPMENT_BUILD-only, explicitly launched, uses memory-only privacy/progress fixtures and Google test ads. It is absent from production.

## Policy and release

Updated policy source: `privacy-policy.html`. Public URL: https://cj.suri.house/alien-pet-privacy . The text distinguishes existing releases from supporting Android builds and describes optional adult Tenjin without authorizing minors' tracking. Deployed first as `site-v1.0.6` (PR 2), then clarified required Google App Set processing in **`site-v1.0.7`**, commit `eb5c010504868d027160f119071bd6aaa4f5e135`, via [PR 3](https://github.com/aurora-teamplayer01/g005-color-drop/pull/3) and [successful deployment](https://github.com/aurora-teamplayer01/g005-color-drop/actions/runs/44228117835). Public Alien Pet page verified HTTP 200 with the new date/child banner/Tenjin clauses; CI also verified existing homepage/terms/privacy and CSS unchanged apart from release metadata.

Release prerequisites: native test banner renders; protected cases make no ad/analytics/attribution requests; release artifact has correct native manifest/dependencies; validate current Play Data safety against actual SDK processing. Verify production banner fill and aggregate revenue after approved rollout; a test banner does not prove revenue. App distribution is US-only; reassess before expanding markets. No behavioral measurement or child CPI claim.

## Unity support

Request submitted successfully (screenshot `unity-request-sent.png`); awaiting Unity's answer. No campaign activation or budget changes. See `Docs/Marketing/AlienPet/UnityAds/2026-11-18/support-request.md`.

## Sources

- https://support.google.com/googleplay/android-developer/answer/9893335
- https://developers.google.com/admob/unity/targeting
- https://support.google.com/admob/answer/6223431
- https://support.google.com/googleplay/android-developer/answer/12955712
- https://www.ftc.gov/business-guidance/resources/complying-coppa-frequently-asked-questions

## Native defects found and corrected during this task

1. `MobileAds.initialize() must be called prior to enable/disable the publisher first-party ID`: defer only the nullable publisher-ID setter until initialization completes, before provider construction/loading. Child/G/non-personalized configuration remains pre-init.
2. `NoClassDefFoundError: com/google/android/gms/appset/AppSet` from AdMob worker: the prior Tenjin-driven global Gradle exclusion is incompatible with GMA 25.4.0. The owner explicitly approved restoring the official Google dependency, retaining all other restrictions. The build removes the old exclusion rather than inventing an SDK shim. A missing-dependency guard also prevents a future misconfigured package from crashing gameplay.

The first two emulator attempts failed for these reasons; Editor test success alone was insufficient. Final native result is recorded separately below. Do not treat those failed attempts as a banner success.

## Final native result

Final APK: `Builds/SpatialLab/Android/COghe-Spatial-Profiler.apk`, SHA-256 `9f63972e396888a20ecd304711055c556de95374520d987a76ebec4125109f6e` (development/test ads, not a store release).

Android emulator completed the explicit memory-only fixture: child test banner rendered on Menu, Firebase/Analytics and rewarded/interstitial remained off, the real Victory page had banner height 0 and no reward-ad offer, and changing to Under9 removed ad eligibility. See `native-result.txt`, `native-banner-observation.png` and `native-child-victory.png`. The final process had no fatal exception, AppSet class failure or AndroidJavaException. The harness enables production privacy UI while disabling save persistence; it does not modify real player progress.

Manifest checks found no advertising-ID/AdServices permissions and no Firebase/MobileAds auto-init providers. `apk-audit.json` records the artifact and checks. Google App Set is present as required by the approved official dependency restoration. This is not a claim of zero Google technical data processing or a complete network audit.

OPPO remained unauthorized during this verification; its Google Play installation and player data were left untouched. Native validation here was on the emulator, not an OPPO performance test. The test app was stopped afterward. Production banner fill/revenue and Unity install attribution are not yet verified; the Unity campaign remains inactive. No game commit/push or Play rollout was performed in this task.

## Follow-up physical OPPO verification

After USB authorization, the same fixes were verified on the real OPPO using Google Play Internal version 7. See [OPPO report](../AlienPetChildAdsOppo20261118/README.md). The fixture now also supports an explicitly flagged Internal QA build, excluded from ordinary production builds. The earlier unauthorized-device limitation above describes the initial session only.
