# Tenjin / Unity Ads verification — 17 November 2026

Status: **incomplete; SDK ingestion blocked by error 202**. The repository configuration disables Tenjin (`androidEnabled: false`). Unity campaign is paused, has spent $0, and has no attribution links. Do not launch or distribute the diagnostic APK as a production build.

## Completed

- Created Tenjin account using the authorized Google account and registered Alien Pet (`com.gravityboxlab.venom`).
- Connected Unity reporting through the service account **Tenjin Alien Pet Reporting**, with only **Advertise Stats API MMP Viewer** permission. Credentials are stored in Tenjin, not this report.
- Configured an install callback for Unity Game ID 800440996, restricted to installs attributed to this channel. Other callbacks remain off.
- Integrated pinned official Unity SDK 1.23.2 / Android SDK 2.0.2, lazy initialization, consent changes and bounded level event forwarding. No iOS runtime activation.
- Passed 21/21 EditMode tests and built an ARM64 IL2CPP diagnostic APK. Tests verify local logic; they do not prove attribution.
- Verified on the emulator that SDK initialization starts only after the user explicitly allows analytics. The request reached Tenjin, which rejected it with `202: invalid device identifier`.
- Sent the user-authorized support email to support@tenjin.com. Automated receipt arrived at 11:26 local time; no technical resolution received at handoff.

## Privacy and test limits

Tenjin requires age 18+ and explicit analytics consent in this implementation. Unknown age, minors and declined analytics do not initialize it. The existing child protections remain. AD_ID / AdServices permissions remain absent. No GAID, App Set ID, hardware identifier or IP fingerprint parameter was sent in the inspected request. Optional parameters are limited to referrer, platform, app version and tracking-limit status.

The earlier emulator APK was sideloaded and had no Google Play Install Referrer. The subsequent real Google Play update now provides a nonempty referrer, but still receives error 202. This establishes rejection of the tested restricted configuration even with a Play referrer; it does not establish that every possible supported no-AD-ID configuration is impossible. Vendor confirmation is required.

Initially, Android rejected the diagnostic APK (code 1) over OPPO's Play build (code 4). OPPO was subsequently updated to version 5 through Google Play without uninstalling or clearing data. No Tenjin ingestion success on OPPO is claimed.

An existing chapter-proof diagnostic about level 30–40 solution coverage appeared during the build; the build still succeeded. No gameplay code was changed to address that separate diagnostic.

## Resume checklist

1. Read Tenjin's technical reply to the subject **Alien Pet Android integration: error 202 and Install Referrer without Advertising ID**. The exact sent text is in `support-request.txt`.
2. Confirm a supported no-Advertising-ID configuration and test method. Do not introduce App Set ID or fingerprinting as an attribution workaround, or broaden collection without an explicit design/privacy review.
3. Apply the supported fix, rerun consent tests and inspect the final APK permissions and native payload. Review public privacy disclosures and Google Play Data Safety before production rollout; neither was updated in this task.
4. Internal version `1.0.4-tenjin-test (5)` is now available. The Google Play update and installer have been verified while preserving OPPO data. This upgrade alone does not prove a fresh attributed install. Use a separate clean test device/profile or vendor-approved reset for attribution, as detailed in `play-test-plan.md`.
5. Verify accepted first-open, permitted level events, consent revocation, install deduplication and Unity install postback. Retrieve Tenjin tracking links only after SDK acceptance and test them in Unity.
6. Recheck creative approvals, budget, account balance and launch readiness. Both creatives currently show Pending. Campaign is US, $9.50/day, $66.50 total, $1 maximum CPI bid; it has not launched.

## Evidence

- `report.json`: build, test and device results.
- `native-connect-redacted.txt`: selected native request/response evidence with identifiers removed.
- `support-request.txt`: exact support message; no keys, raw logs or device identifiers.
- `support-received.jpg`: confirmation of support receipt.
- `Artifacts/Tests/Tenjin/results-final.xml`: 21 passing tests (local artifact).

These changes are uncommitted on branch `IOS` at this handoff. No production release or campaign activation occurred.

## Support follow-up and Internal release

- Follow-up sent in the original support thread at 11:41 Asia/Saigon on 17 November 2026. It asks Tenjin to reconcile SDK mandatory Advertising ID with S2S opt-out/referrer documentation and confirm a non-probabilistic test flow. See `support-followup.txt` and `support-followup-sent.png`.
- Built and statically validated signed ARM64 IL2CPP Release AAB `1.0.4-tenjin-test (5)` with the existing upload key. Same runtime source as the 21 passing consent tests. See `play-build-report.json`, privacy/config validation reports and `play-config-restoration.json`.
- Published only to the existing Internal testing track at 11:51 local time. Console shows **Available to internal testers**; existing OPPO QA tester list unchanged. Matching native symbols uploaded. No production promotion.
- Test link: https://play.google.com/apps/internaltest/4700437523599384969
- Tenjin enabled only in the packaged diagnostic AAB; repository configuration restored to disabled. Adult explicit consent remains required. No AD_ID/AdServices permission added.
- Console warnings: existing Advertising ID declaration conflicts with the intentionally absent permission; no R8 mapping because minification is disabled. No declaration or release-blocking setting changed.
- OPPO was subsequently updated from Play version 4 to Internal version 5; see the physical-device result below. No uninstall or data clearing occurred.
- Support resolution, accepted attributed install and Unity postback remain pending. Campaign stays paused.

## Physical OPPO Google Play result — 17 November, 12:08–12:16

**Installation passed; Tenjin ingestion failed. Campaign remains paused.**

- Google Play Internal Beta updated the existing installation to `1.0.4-tenjin-test (5)` at 12:08:34. Android reports installer `com.android.vending`; first-install time remained 2 November 2026 08:55:28. No sideload, uninstall, data clear or save reset.
- Menu immediately after update showed level 12, 300 Drops and the mint pet. After the user's gameplay it showed level 14 and 320 Drops. Screenshots record both states.
- Privacy was observed at adult 18+ / Analytics On during testing. The native connect payload had a nonempty `referrer`, `referrer_click_ts` and `referrer_install_ts`, with `advertising_id` absent. Repeated server replies, including 12:13:42, were `{"code":202,"message":"invalid device identifier"}`.
- After the user played, Tenjin Apps still showed **Sending Events: no**. No accepted gameplay event, attributed install or Unity postback was verified. The retained native log contains connect attempts, not an independently verified custom-event acknowledgement. Do not equate the native `/v0/event` URL for connect with accepted gameplay events.
- This was an update of an existing Play install, not a fresh-install attribution test. No progress was sacrificed to create a clean install.
- **SDK privacy finding:** native diagnostics showed a successful local App Set ID fetch after adult consent. That identifier is absent from the inspected connect payload, but parameter allowlisting does not establish absence of local SDK collection. Resolve this before production; do not use App Set ID as an advertising-attribution workaround. The actual identifier was not retained in report evidence or sent to support.
- AD_ID and AdServices permissions remain absent from the installed package. No child gating, age choice or identifier collection setting was broadened to overcome error 202.
- At the end, the user confirmed Analytics was switched Off. The final screenshot shows the menu rather than the Privacy panel, so this last choice is user-confirmed, not independently visually verified. No later connect appears in the retained log snapshot; this short observation is not a comprehensive revocation test.
- Authorized support follow-up sent at 12:15 with the Play-referrer/error-202 result and the local App Set ID finding. No credentials, raw logs or device identifiers were sent. A technical vendor response remains pending.
- Mirror coordinates were unreliable. A mistaken tap temporarily left the Internal tester program; enrollment was restored using keyboard navigation before the successful Play update. No game data was changed by that enrollment detour.

Evidence: `oppo-play-result.json`, `oppo-native-redacted.txt`, `oppo-play-updated.png`, `oppo-progress-preserved.png`, `oppo-play-level14.png`, `oppo-progress-after-test.png`, `tenjin-play-no-events.png`, `support-play-result.txt`, `support-play-result-sent.png`.

Next: obtain Tenjin's supported no-AD-ID/no-probabilistic-matching configuration and local-collection controls, then rerun native ingestion and consent-revocation tests. Only after acceptance, use a separate clean test device/profile for attributed install and Unity postback verification. Do not activate the campaign or promote this diagnostic build to Production yet.

## Follow-up: linked guide audit (17 November, afternoon)

Read the expanded sections of [Check SDK Status](https://tenjin.com/docs/check-sdk-status/), the [Channel Setup directory](https://tenjin.com/docs/category/channel-setup/), its [Unity Ads guide](https://tenjin.com/docs/unity-ads-setup/), and [Self-Serve Billing](https://tenjin.com/docs/self-serve-billing-guide/) in Chrome. These are substantive setup checklists; the onboarding email itself is not a technical resolution.

- The SDK guide explicitly describes Android Install Referrer as usable when Advertising ID is unavailable. Both generated Gradle project and source dependencies already include `com.android.installreferrer:installreferrer:2.2`; the SDK supplies ads-identifier 18.1.0. The prior Play payload confirms a real nonempty referrer. Adding the same dependency again is not a demonstrated fix.
- Followed Apps → Alien Pet → SDK → Tenjin SDK Integration Status. Actual result: **No SDK calls received**. This is not merely a missing custom event or a filtered general report.
- Live Event registration requires Android GAID; registered test-device events are excluded from general dashboard reports, and the guide allows up to three hours after device registration. No test device/GAID was registered or transmitted. The previous native 202 response independently establishes rejection; a blank Live Event view alone would not establish it.
- Checked Callbacks → Unity Ads: Game ID 800440996, install callback On for this channel only; additional revenue/open/IAP/subscription callbacks remain Off. No broader sharing enabled.
- Existing Tenjin campaign details explicitly block tracking-link generation until the app sends events. Unity campaign ID remains unknown in Tenjin; link/postback validation is still pending, not complete channel attribution. Screenshot: `tenjin-tracking-gated.png`.
- Followed My Account → Usage & Billing: Free 2,000 conversions/month, **0/2,000 used**, no payment method. The account UI lists $0.04/additional conversion. No upgrade, payment entry or subscription action was performed. Screenshot: `tenjin-billing-free.png`. The available evidence does not support billing/quota as the cause of error 202.
- The pinned Unity SDK README's parameter-allowlist section still calls Android `advertising_id` mandatory, whereas the web checklist describes referrer fallback. The exact supported no-AD-ID payload/opt-out configuration remains unresolved. No collection or child protections were broadened based on this documentation ambiguity.

No runtime change or rebuild was justified by this checklist audit. Existing support requests already ask for the supported no-AD-ID configuration, suppression of local App Set ID collection and a valid attribution test procedure. Campaign remains paused pending acceptance and attribution verification.
