# Alien Pet Google Play release preparation — 29 October 2026

## Current status — 1.0.1 (2), United States only

**Version 1.0.1 (code 2) is uploaded. Production is now prepared in Publishing overview, with 10 changes not yet submitted; internal testing remains a saved draft. No review submission or rollout has occurred, and the app is not published.** The user selected **United States only**, superseding the earlier all-country proposal; Console shows **Targeted (1): United States**.

The production release was saved as **1.0.1 (2) - US launch draft** and is now listed in Publishing overview with the pending action **Start full rollout**. That label is not an actual rollout. Quick checks are complete: Console says **“Your changes can now be sent for review. We may find additional issues when reviewing your app.”** The **Submit 10 changes for review** button is enabled; neither submission nor final rollout was clicked by the agent. Managed publishing is off. The release details show heading **1.0.1**, bundle **2 (1.0.1)** with native symbols, and **Not yet sent for review**. The internal draft contains only version 2; version 1 is retained as historical build evidence below. All **11 initial app-setup items** were completed, and production-release preparation is unlocked. Data safety and the English store listing are saved as ready to send for review. These are saved declarations and drafts, not Google approval or a compliance certificate.

- [Production release preparation](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/tracks/4698253974970298717/releases/1/prepare), [saved-draft proof](console/release-1.0.1-2-production-saved.jpg), [US-only country proof](console/us-only-saved.jpg).
- [Current Publishing overview](console/publishing-ready-10-changes.jpg), [full-page proof](console/publishing-ready-full.jpg), [production prepared / not submitted](console/production-prepared-not-submitted.jpg).
- [Internal draft](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/tracks/4700437523599384969/releases/1/prepare), [save confirmation](console/release-1.0.1-2-internal-saved.jpg).
- [Data safety](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/app-content/data-privacy-security?source=dashboard), [saved proof](console/data-safety-ready-for-review.jpg), [current setup state](public-setup-state.json).
- [Target audience](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/app-content/target-audience-content?source=dashboard), [saved age groups](console/target-audience-9-15-saved.jpg).

Google Play Games on PC was opted out, removing the pending PC change. The Android XR same-track default was inspected and left unchanged. This remains a mobile phone/tablet release; this Console continuation did not rebuild the AAB or fix campaign routes.

The user approved **9–12 and 13–15** as the launch target groups and approved the Families declaration. This replaces the earlier preschool-inclusive design brief for launch targeting. Under-13 runtime protections still cover every under-13 player, not only ages 9–12. The app is **not in the Teacher Approved program**.

| Current artifact | Result |
| --- | --- |
| Package | `com.gravityboxlab.venom` |
| Version | `1.0.1` (code `2`, already used in Play Console) |
| Signed AAB | `Builds/AlienPet/GooglePlay/AlienPet-1.0.1-2.aab` |
| AAB bytes | 58,684,797 |
| AAB SHA-256 | `30bc7fce27fda89f484a35bc776bfdbc312b99daec30e519d4dc2bc9fb2d9ab0` |
| Derived release APK SHA-256 | `2c421b502a0e405f1588f044ca784caaf5c3b28c11998216717350d7c7d01b4f` |
| Verification | [Native release privacy results](native-privacy-results.json) |

The SHA identifies the AAB from which the signed release APK was generated. Version 2's native manifest, certificate match, release flags and APK 16 KB ZIP alignment were checked separately. Do not transfer every version-1 validation result below to version 2 without its own evidence.

Native debug symbols for the **exact version-2 build** were uploaded and attached in Play Console, then the production draft was saved. The ZIP SHA-256 is `89fcea49f041ef9da3bcb1a0033d90aa06ebefb2f32090e282f13aa0ecaaeb5c`. It covers **7 of 11 ARM64 libraries** with matching build identity/code evidence; these are public function-name symbols, not complete source-file/line coverage. Three Firebase libraries and one datastore library remain without verified symbols. **R8/ProGuard mapping does not apply** because release minification was disabled. See the [compact symbol audit](native-symbols-summary.json) and [Console attachment proof](console/native-symbols-attached.jpg).

The **Advertising ID declaration** is saved as **Yes**, with Analytics (AdMob measurement), Advertising/marketing, and Fraud prevention/security purposes. The missing-`AD_ID` release-error check is disabled intentionally: version 2 removes that permission, preventing Advertising ID access on Android 13+, while eligible AdMob operation on supported API 26–32 may still use it. This declaration does not enable optional gameplay Analytics or Ads for under-13 players. See [saved declaration](console/advertising-id-saved.jpg) and [Google Mobile Ads disclosure](https://developers.google.com/admob/android/privacy/play-data-disclosure).

## Privacy implementation and verification — current release

The neutral age screen runs before SDK initialization. Unknown and under-13 players initialize no Ads, Firebase Analytics or Remote Config SDK. Ages 13–17 use restricted contextual-ad treatment, with no Analytics or Remote Config. Adults receive a separate optional Analytics choice; declining still permits gameplay. Privacy controls and the policy link remain available, and adults can withdraw Analytics consent. Advertising consent is separate from Analytics opt-in.

**14 targeted Unity tests passed: 10 EditMode service tests and 4 PlayMode privacy/UI tests.** The signed 1.0.1 (2) release was also tested on an isolated Android 36 emulator at tablet landscape, tablet portrait and phone sizes. Bounded observations found no app-UID network traffic or SDK artifacts before confirmation and for under-13 play/relaunch. Adult pending/decline/allow/withdrawal and both teen bands were exercised. Withdrawal persisted across relaunch. These observations do not establish all-device, all-jurisdiction or long-duration certification; shared Google Play services traffic is not fully attributed by app-UID counters. See [implementation](privacy-implementation.json), [native plan](native-privacy-plan.json) and [native results](native-privacy-results.json).

The [public privacy policy](https://cj.suri.house/alien-pet-privacy) is deployed, and its one-file source change was merged into the website repository's `main`; no unrelated website/game changes were merged. See [merge verification](privacy-merge-readiness.json), [live checks](privacy-live-checks.json) and [request-handling operations](privacy-request-operations.txt). Support must operate the published request process; stopping collection is not deletion of data already received by a service.

Google Analytics Data Processing Terms were accepted after the user's explicit action-time confirmation and verified after reload on **29 October 2026**. Sharing and retention preferences were preserved. The [dashboard audit](analytics-console-audit.json) records event retention of 2 months, user retention of 14 months with activity reset, Google signals off, Google products/services sharing off, and no Google Ads or AdMob Analytics links. Its limited conclusion for optional adult gameplay events is **collected, optional, not shared** under the applicable service-provider/fully-anonymized exceptions; this does not apply to AdMob operational data. [DPA acceptance proof](console/analytics-dpa-accepted.jpg).

## Content ratings — submitted questionnaire

IARC submission is complete. The current declaration includes rare frightening fantasy imagery and comic belching, no blood/gore, and no functioning real-money purchase provider. The following are the confirmed key regional results returned by Console; the saved certificate and Console remain authoritative for other regions.

| Authority / region | Rating | Returned descriptors |
| --- | --- | --- |
| ESRB / United States | Everyone | Mild Fantasy Violence; Comic Mischief |
| PEGI | 7 | Fear |
| Taiwan | 12 | Horror |
| USK / Germany | 6 | — |
| Australia | PG | — |

These are regional content ratings, distinct from the declared design audience. The US-only launch uses the ESRB result; the table does not claim that the game is suitable from one universal minimum age. Do not alter truthful questionnaire answers to force a lower rating. See [completed IARC proof](console/iarc-completed.jpg), [content audit](families-content-audit.json) and [Google's rating guidance](https://support.google.com/googleplay/android-developer/answer/9859655?hl=en).

## Historical validation — initial 1.0.0 (1)

The following build and tablet results describe the **initial version 1.0.0 (1)**. They are retained as historical evidence and do not replace the current version-2 privacy results or establish review approval. This is an Android release, not an iPad/App Store binary.

### Initial Google Play result

The user confirmed the two Create app declarations. **Alien Pet: Escape Puzzle** was created under **YoloStudioGlobal** (developer `5637064096940148363`, app `4974079895895643247`). Google Play processed version **1.0.0 (1)** and saved it as an **Internal testing draft**. No tester rollout, review submission or public release was performed.

The English default store listing was saved with its title, short/full descriptions, approved icon, feature graphic, six phone screenshots and four screenshots for each of the 7-inch and 10-inch tablet slots. Its observed status is **Ready to send for review**. The generated icon and feature graphic were individually labelled as AI assets; screenshot provenance retains the actual game renders and code-composed marketing layout.

- [Saved release](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/tracks/4700437523599384969/releases/1/prepare)
- [Store listing](https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/store-listings)
- [Historical initial Console state](console-state.json), [AAB proof](console/aab-version-saved.jpg), [listing proof](console/store-listing-ready.jpg)
- [ASO gallery](../../Marketing/AlienPet/GooglePlay/2026-10-29/screenshot-review.html) and [upload pack](../../Marketing/AlienPet/GooglePlay/2026-10-29/AlienPet_GooglePlay_UploadPack_2026-10-29.zip)

### Initial Android tablet runtime check

The signed store APK derived from this AAB passed an isolated Android 36 tablet emulator smoke test: first-run intro/Skip, tap movement, camera orbit, native two-pointer pinch zoom, level 1 wall-exit solve, victory-to-level-2 transition, Pause and Main Menu without test controls. Separate 1600×2560/sw800dp and 1200×1920/sw600dp captures exercise the two tablet layouts. The victory UI fixture reported zero measured dance/text/button overlaps at both sizes.

Later-level screenshot staging used the existing test APK with its level picker closed before capture, separately from the signed store smoke. These are emulator/UI results, not physical-tablet FPS certification or an all-level native solve pass. See the [tablet audit](../../Marketing/AlienPet/GooglePlay/2026-10-29/qa/tablet-readiness-audit.json) and [capture provenance](../../Marketing/AlienPet/GooglePlay/2026-10-29/qa/tablet-capture-manifest.json).

### Initial 1.0.0 (1) artifacts

| Item | Result |
| --- | --- |
| App | Alien Pet: Escape Puzzle |
| Package | `com.gravityboxlab.venom` |
| Version | `1.0.0` (code `1`) |
| AAB | `Builds/AlienPet/GooglePlay/AlienPet-1.0.0-1.aab` |
| AAB bytes | 58,677,976 |
| AAB SHA-256 | `218e4cc1646b5d9add183785661ee7526e2e7dd7fc0ddd3f1178e365b69ecfbe` |
| Universal test APK | `Artifacts/AlienPetPlayStore/AlienPet-1.0.0-1-universal.apk` |
| APK bytes | 59,027,763 |
| APK SHA-256 | `9ffce5bb32cde58a93bb8b855fb5aae95214558a7acbf4a40a3763f72e12989a` |
| Upload certificate SHA-256 | `5b894271027148d895e3c1bc0f84f0490e0c78f3f745ebeafa118f82dc0a3d4c` |

The universal APK was generated from this AAB for isolated Android tablet emulator testing. APK generation made no device calls. Device interaction and runtime findings are a separate verification step; no physical phone was reinstalled by the build workflow.

### Initial 1.0.0 (1) verified properties

- Unity **6000.3.19f1**, IL2CPP **Release**, **ARM64**, minimum API **26**, target API **36**, portrait.
- Existing approved app icon and current **67-entry** Spatial catalog retained: 65 campaign entries plus F2/F3. No level regeneration or gameplay edit was part of this build.
- `bundletool validate` passed. JAR signature matches the app-specific upload key; SHA-256 digests were independently checked for all **1,066 signed content entries**.
- All **11 native ELF libraries** have minimum `PT_LOAD` alignment of **16,384 bytes**. Bundle configuration specifies `PAGE_ALIGNMENT_16K`; the derived universal APK also passes `zipalign -c -P 16 -v 4`.
- Manifest `debuggable` and `testOnly` are false. Build uses `BuildOptions.None`, no extra scripting defines, and disables development, script debugging, profiler connection and deep profiling settings. The store path rejects test/benchmark/development symbols and benchmark/profile environment flags.
- Exact diagnostic type names are absent from packaged IL2CPP metadata: mobile UI/services proofs, view replay, brand proof, advertising/care reels and hardware benchmark. A retained source filename for the always-present metrics helper is not a compiled benchmark type.
- Firebase project/app and AdMob application IDs match the existing `com.gravityboxlab.venom` configuration. All three production ad unit IDs are packaged.
- Android large/xlarge screens are enabled; there is no explicit compatible-screen whitelist or required telephony/camera/GPS hardware. Touchscreen and Vulkan features are marked optional. The player activity is resizable. These manifest checks do not establish tablet UI quality.
- Temporary Player/Editor settings were restored. The outer runner restored the original `ProjectSettings.asset` bytes after build. Generated Gradle signing text was scrubbed; a post-build scan of 42 generated/configuration files found no signing passwords.

Public machine-readable results: [bundle validation](build-validation.json) and [universal APK validation](universal-validation.json). Detailed local logs, the public certificate and helper scripts are under ignored `Artifacts/AlienPetPlayStore/`.

The build also serialized default `VisionOS` and `tvOS` TextureImporter entries in 11 snack/toy/HAT_BEE icon `.meta` files. Both new entries have `overridden: 0` and match each file's existing default-platform settings. Existing Android/iOS settings and GUIDs are unchanged; these additions do not change Android import behavior. Build-log import records and file diffs establish their origin. The audit left Assets untouched. See [metadata import audit](meta-import-audit.json).

The JDK verifier reports a self-signed upload certificate and AAB ZIP entry-order warnings for streaming JAR readers. Its normal JAR verification passed; all packaged content digests were checked independently and bundletool accepted the bundle.

## Future build command

The checked-in entry point is `GravityBox.Editor.COghePlayStoreBuilder.Build` in [COghePlayStoreBuilder.cs](../../../Assets/_Game/Editor/COghePlayStoreBuilder.cs). It requires process-local signing credentials, version and output settings; it does not store credentials in the project or create a release in Play Console.

The local ignored helper reads the existing private credential JSON, removes benchmark/profile environment flags, invokes Unity, restores build-side effects and scrubs temporary signing text. After confirming the next unused code in Play Console, a future build can use:

```sh
ALIEN_PET_VERSION=1.0.2 ALIEN_PET_VERSION_CODE=3 \
  python3 Artifacts/AlienPetPlayStore/build_store.py
```

Version code **2 is already used**. `1.0.2` and code `3` are the next-build example, not a reservation; recheck the next unused code before building. The helper refuses to overwrite an existing versioned AAB. It is a local ignored file, so a new checkout must recreate the wrapper or provide the same process environment securely. Do not put passwords directly in shell commands or commit them.

Required process environment for the checked-in builder:

- `COGHE_STORE=1`; `COGHE_BENCHMARK` and `COGHE_PROFILE` disabled.
- `ALIEN_PET_VERSION`, `ALIEN_PET_VERSION_CODE`, `ALIEN_PET_AAB_OUTPUT`.
- `ALIEN_PET_KEYSTORE`, `ALIEN_PET_KEY_ALIAS`, `ALIEN_PET_STORE_PASSWORD`, `ALIEN_PET_KEY_PASSWORD`, supplied by a private wrapper. The builder clears its signing environment variables before building and restores its previous signing settings afterward.

The local initial-artifact validators are `Artifacts/AlienPetPlayStore/validate_store.py` and `Artifacts/AlienPetPlayStore/generate_universal.py`; their artifact/version expectations must be updated for a later release. The APK generator uses temporary mode-0600 password files outside the repository and removes them in `finally`.

## Upload-key backup and remaining checks

The new per-app upload key exists outside Git:

- `/Users/mrk/.android/alien-pet-upload/alien-pet-upload.jks`
- `/Users/mrk/.android/alien-pet-upload/credentials.json`

The directory is mode **0700** and both files are **0600**. Keep an encrypted secure backup of **both files** before relying on this key for future updates. Neither file should be committed or attached to a chat. The public certificate is safe to use for fingerprint comparisons. Google-managed Play App Signing is separate from this local upload key.

## Release-readiness gaps

- **Ad serving remains unverified and blocked by missing Alien Pet UMP forms.** The normal AdMob European/US message editor stalled while loading and the final retry again remained blank; no Alien Pet form was created or published. The existing Critter Jam message was not changed. Signed release 1.0.1 (2) receives UMP publisher error 3 and correctly fails closed, leaving gameplay available. Automatic Analytics consent propagation remains off. See [AdMob setup evidence and recovery steps](admob-consent-setup.json). Resume the normal editor when it works, publish the app-scoped forms, then rerun eligible native consent/ad flows; do not bypass the error.
- A real IAP provider is not installed. Purchases are intentionally unavailable in the store build.
- The prior continuous native campaign run remains **57/65 authored routes passed**. Display levels **10, 18, 27, 32, 48, 51, 56 and 57** remain unresolved in that run. Isolated rechecks do not replace those results; this bundle validation is not an all-levels-passed result. See [native full-run review](../AlienPetOptimization20261029/native-soak-review.md) and [recheck audit](../AlienPetOptimization20261029/recheck-audit.md).
- Privacy policy, Data safety, target audience, Families declaration and IARC setup are now saved. The US-only production release has passed Console quick checks and is ready to submit from Publishing overview, but no review submission, approval or rollout has occurred. Console configuration readiness does not establish working ad delivery or completion of all 65 campaign routes. Resolve the outstanding runtime/ad checks before treating those as verified.

Tablet runtime findings and Console processing are recorded separately. No production rollout is established by this document.
