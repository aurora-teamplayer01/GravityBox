# Alien Pet 1.0.3 (4) — Google Play upload

Built 8 November 2026 from the current workspace with the ads tracking fixes. Uses the same upload certificate as version 3. No gameplay or privacy rules changed by this release task.

- AAB: `Builds/AlienPet/GooglePlay/AlienPet-1.0.3-4.aab` (58,687,673 bytes).
- Release: ARM64 IL2CPP, non-debuggable, target API 36 / minimum 26.
- Bundletool, signature, all signed-entry digests, 16 KB alignment and service IDs validated.
- 67 catalog scenes; no QA/benchmark types. Production AdMob units packaged.
- `rewarded_earned` and expanded load-error diagnostics verified in packaged metadata.
- Previous verification: [12 passing tests and native SDK/Firebase reward proof](../AlienPetAds20261108/review.html).
- Uploaded exact-version public native symbols for 7 libraries, with matching Build IDs and code ranges. Some third-party binaries do not have verified symbols.
- Play retains support for 8,479 phone and 4,761 tablet models; zero removed.
- Two nonblocking warnings remain: existing AD_ID declaration/permission mismatch and absent R8 mapping (release minification disabled). No consent or child protection weakened to remove warnings.

Production change saved for the owner to submit through Publishing overview. This task does not submit for review or roll out the release. Existing countries and unrelated changes already in review are preserved.

Live No fill is not guaranteed resolved. This version adds request diagnostics and corrects the reserved Firebase event name; test ad revenue is zero and is not evidence of production earnings.

See `validation-report.json`, `release-record.json`, `release-notes.txt` and the Console screenshot for evidence. Detailed build and signing validation logs are local under `Artifacts/AlienPetRelease20261108`.
