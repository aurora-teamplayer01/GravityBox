# First Understand COghe — native Google Play review

Owner approved the official Google review sheet directly on 20 November 2026, replacing the original proposal to filter 1–3 versus 4–5 stars. Owner also requested OPPO testing followed by a Production submission. Unity Ads testing is suspended; do not add funds or enable the campaign.

## Runtime contract

- At the first newly completed Understand COghe interlude (currently practice 1001 after display level 6), keep the existing happy creature, celebration and reward summary.
- The existing **Continue** button requests the official Google sheet before the earned ability card. No extra rating button, preliminary star picker, sentiment question, reward or forced store redirect.
- Google's UI is unmodified. The game does not know the chosen score, whether a sheet appeared, or whether a review was submitted. No claim or thank-you implies a rating was recorded.
- Persist one request opportunity with the existing local shop save, consuming it before the asynchronous request. Replaying lessons or reopening the app cannot repeat a consumed request. Legacy completed lessons do not generate a retroactive request.
- The request phase times out after 8 seconds without launching later. Suppression, errors and dismissal return to the same earned card/progression. While the native flow is active, Continue and game input cannot advance underneath it.
- Review code never changes age, Analytics or Ad measurement consent, never initializes an advertising/attribution SDK, and never sends a rating analytics event. Existing child protections are unchanged.
- Android registers the provider; other platforms keep normal continuation. Google controls account eligibility, quota and availability.

## Implementation

Official OpenUPM package `com.google.play.review` 1.8.4; dependencies `com.google.play.common` 1.9.2 and `com.google.play.core` 1.8.6. Native libraries: Play Review 2.0.2, core-common 2.0.4. The Android provider has its own assembly and is independent of Firebase/AdMob/Tenjin bootstrap.

`ALIENPET_REVIEW_QA` includes an explicit one-shot native probe for the existing OPPO save. It only runs when launched with the `alienpet_review_probe` test intent, does not alter progress or the persisted review opportunity, and never submits a rating. The Internal builder requires a `-review-qa` suffix; ordinary Production builds exclude this type/define. Do not promote the QA bundle.

## Verification

- 38 EditMode tests passed: first-completion bookkeeping, reload/replay, legacy saves, skipped practices, privacy and attribution regression tests.
- 4 PlayMode tests passed: real first lesson → review → ability card; store failure; quota/no-sheet return; existing first-practice recovery/card-resume flow.
- 8 additional privacy UI tests passed, including neutral age gate, child/teen restrictions and separate revocable adult measurement consent.
- OPPO baseline before update: version 9 from Google Play, original first-install time 2 November; main menu **level 18 / 275 Drops**, existing pet appearance intact.

### OPPO native review verification

- Internal `1.0.9-review-qa (10)` published and updated through **Google Play**, installer `com.android.vending`; original first-install time remains `2026-11-02 08:55:28`.
- Explicit test probe displayed the **real Google Play review sheet** on OPPO CPH2591. Screenshot: [native sheet](oppo-native-review.png).
- Pressed **Not now** without selecting stars or submitting any review. Provider returned `NoError`, the probe completed, and the menu remained at **level 18 / 275 Drops** with the original pet appearance.
- The probe verifies the actual Android API independently of the milestone; the first-lesson → Continue → review → ability-card sequence is covered by PlayMode tests. Existing OPPO progress was not cleared or forced back to the first lesson.
- Log evidence: `Artifacts/AlienPetReview20261120/oppo-native-review.log`.

### Initial Production candidate (superseded)

- `1.0.9 (11)`, `Builds/AlienPet/GooglePlay/AlienPet-1.0.9-11.aab`.
- SHA-256: `54a14d838e47f2f727b000e81a538c22606f9bef4dc28025a99b6b1e1f41f0ad`.
- Same upload key verified; 67 scenes; ARM64 Release; SDK 26–36; tablet support; 16 KB ELF and bundle alignment passed; no debuggable/testOnly flag or AD_ID permission.
- Official review provider present; `AlienPetReviewProbe` and gameplay diagnostic types absent. Production does not enable adult attribution QA mode.
- Exact native symbols verified using GNU Build IDs and executable text matches for seven libraries, uploaded to Play for version 11. Four third-party libraries have no verified symbol counterpart; no claim of full stack symbolication. R8 is disabled, so no mapping.txt exists.
- Validation artifacts: `Artifacts/AlienPetReviewProduction20261120/validation-report.json` and `native-symbols/native-symbols-audit.json`.
- Version 11 was published to Internal only. Its unsent Production release was discarded after Play's new quick check flagged an implicit portrait hardware requirement. The uploaded artifact remains in the library; it was not submitted to Production.

### Device-availability correction and final candidate

- Google Play now detects the implicit `android.hardware.screen.portrait` requirement from the activity's portrait orientation. Rather than ignore the warning, the Android postprocessor explicitly marks that hardware feature optional. The Unity activity still requests portrait; graphics, camera and gameplay are unchanged.
- Two regression cases passed for absent/explicit prior declarations, repeated postprocessing and preserving activity orientation and OpenGL requirement (52 passing tests total for this task).
- Final candidate is `1.0.9 (12)`, `Builds/AlienPet/GooglePlay/AlienPet-1.0.9-12.aab`, SHA-256 `a0f848797148f079ced5598e69f9d30b9cb95a8bc9e0bdf43b79ec207b1cae61`.
- Full artifact validation passed again, including optional portrait hardware, portrait activity enum, unchanged child privacy defaults, no AD_ID, no probe, official review provider, signing and 16 KB alignment.
- Exact version-12 symbols are in `Artifacts/AlienPetReviewFinal20261120/native-symbols`; do not use the version-11 symbol archive for version 12.
- Version 12 and its exact symbols were uploaded; Internal release `1.0.9 (12) - Native reviews and device support` is available to testers.
- OPPO updated through Google Play to version 12 (`com.android.vending`, original install time retained). Main menu still shows level 18 / 275 Drops. After the owner tapped Play, level 18 rendered successfully; the current process log contained no matching fatal/Java/NullReference/MissingMethod exceptions. Screenshots: [menu](oppo-production-12.png), [gameplay](oppo-production-gameplay.png). Evidence: `Artifacts/AlienPetReviewFinal20261120/oppo-package.txt`, `oppo-startup.json`, `oppo-gameplay-smoke.json`. This is a final-build startup/gameplay smoke, not a replay of all campaign levels.
- Production version 12 was submitted for full rollout on 20 November, with managed publishing off. Submission opened a warning that the review in progress since 18 November would be cancelled and restarted. Automatic approval review initially rejected that action; no bypass was attempted. The owner then explicitly approved restarting the review, and the action succeeded.
- Publishing overview now lists only `1.0.9 (12) - Native reviews and device support` under **Changes in review**, with **Start full rollout**. Google quick checks are still running; the page says the changes will be sent for review once checks complete successfully. This is a submitted/queued Production release, not evidence that the new version is live or approved. With managed publishing off it will publish automatically after approval. Screenshot: [submitted Production release](production-submitted.jpg).

## Sources

- [Google design and timing requirements](https://developer.android.com/guide/playcore/in-app-review)
- [Official Unity integration](https://developer.android.com/guide/playcore/in-app-review/unity)
- [Official plugin repository](https://github.com/google/play-in-app-reviews-unity)
- [Google guidance on implicit orientation filters](https://developer.android.com/develop/ui/compose/layouts/adaptive/increase-app-availability)
