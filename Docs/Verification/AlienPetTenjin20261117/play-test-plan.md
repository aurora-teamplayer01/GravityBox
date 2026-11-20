# Google Play Internal test — Tenjin

Prepared 17 November 2026. This is an isolated attribution diagnostic, not a production release or proof that CPI measurement works.

## Build contract

- Package: `com.gravityboxlab.venom`; use the existing upload key and Google Play app signing.
- Version: `1.0.4-tenjin-test`, version code `5` (accepted and published to Internal testing).
- ARM64 IL2CPP Release, store UI, no development tools or benchmark automation.
- Tenjin enabled only in the packaged test configuration; restore the repository default to disabled after building.
- No Tenjin initialization for unknown age or under 18. Adults explicitly choose analytics using the new disclosure. Existing refusal and progress must be preserved.
- No AD_ID or AdServices permission. No new identifier or IP-based matching added. No changes to existing child ad protections.
- Unity Ads campaign stays paused until an accepted attributed install and Unity postback are verified.

## Verification sequence

1. Validate AAB structure/signature, same upload certificate, version/package, privacy metadata, permissions and bundled Tenjin configuration.
2. Upload to the existing Internal testing track, restricted to the existing OPPO QA tester. Do not promote to Production.
3. Update the existing OPPO app from Google Play; verify installed version and installer `com.android.vending`. Preserve app data. This checks delivery, upgrade and consent; it is not evidence of a new attributed install.
4. On unknown/minor/declined consent paths, verify no Tenjin initialization or sends. On a consenting adult test, observe whether Play Referrer is available and whether the server accepts the event. Record only redacted evidence.
5. Wait for Tenjin to confirm the SDK or S2S configuration and how to obtain a test link before initial event acceptance. Do not fabricate advertising IDs, consent signals, attribution links or install success.
6. Use a separate clean test device/profile or vendor-approved reset for the actual tracked install. Do not uninstall or clear the user's OPPO save data. Follow a genuine test tracking link, install through Play, launch and consent; check accepted first-open, correct Unity campaign attribution and install callback.
7. Check no duplicate install on relaunch, bounded level events and consent revocation. Only after these pass is CPI launch readiness established.

## Known limits

The earlier sideloaded emulator test reached Tenjin but returned code 202. No Play Install Referrer was available. SDK docs list Android advertising_id as mandatory, while S2S docs allow empty/zero for opt-out; Tenjin has been asked to reconcile this and confirm that probabilistic/IP matching can be disabled. Adding an empty field is not assumed to fix the native SDK.

Support follow-up was sent at 11:41 Asia/Saigon on 17 November 2026, in the original support thread. See `support-followup.txt`.

## Delivery status

Internal release `1.0.4 (5) - Tenjin restricted Internal QA` published 17 November 2026 at 11:51 local time; Console confirms Available to internal testers. Matching native symbols attached. Existing QA tester list unchanged. OPPO upgrade and attributed install tests are pending. See `play-internal-published.png`.
