# Unity install attribution gate — 20 November 2026

Owner decision later on 20 November: **suspend Unity Ads testing** because the stated minimum funding is USD 1,000. Do not continue attribution testing, add funds or activate the campaign unless the owner resumes this task. The findings below remain historical evidence.

**Result: not yet verified. Keep the campaign paused.** Tenjin accepting SDK events is established by the preceding [OPPO test](../AlienPetAdultAttribution20261120/README.md), but this is not proof of a Unity-attributed install or accepted Unity install postback.

## Live dashboard findings

- Unity campaign `ALIENPET_ANDROID_US_CPI_20261117` / `8c410de14eb622e8e5b934e1`: Paused, not launched; impressions 0, installs 0, spend $0.00. Billing balance $0.00.
- Both creative packs, `AlienPet_US_EN_E_Pet_Gameplay` and `AlienPet_US_EN_D_Puzzle_Gameplay`, are Pending.
- **Test attribution link is disabled**. Tooltip requires a click attribution link and an approved creative pack. Existing click/impression links point to Tenjin, retain GAID/gamer ID macros, and omit IP, user-agent and device-model macros. Provider label is Other; support clarification requested.
- Tenjin Unity Ads install callback is active for Game ID **800440996**, matching Unity. Scope is **this channel only**. Other callback types remain off.
- Callback health: **current hour 0 attempts; last day 0 attempts; no recent successes or errors**. This is not an HTTP failure: no callback attempt is recorded. The OPPO install was an update, not a new Unity-acquired installation.
- The generated callback template contains IP/device placeholders. Their presence is not evidence of transmitted values; SDK opt-outs remain in place. No actual postback payload was available to audit.

## Device and privacy constraints

OPPO data was not uninstalled, cleared or reset. Its last verified state remains level 18, 275 Drops, Analytics Off and Ad measurement Off. This follow-up only checked USB availability, not game state mutation. No consent was enabled again.

Only separately consenting adults 18+ may initialize the QA attribution path and use GAID. Analytics alone grants no ad-measurement permission. Minors, unknown ages and refusals remain blocked. Android ID, App Set ID and IP matching remain excluded; no probabilistic matching or broad callback sharing was enabled.

No second physical Android was attached. The existing AlienPetTablet AVD is a Google APIs image with Play Store disabled; it is not ready for a fresh Play-install test. Do not sacrifice the player's OPPO save or reset its advertising ID to manufacture an install.

The installed Internal QA version is **1.0.8-attribution-qa (9)**. This isolated build suppresses Firebase/AdMob/UMP initialization and is **not a production release candidate**; see the preceding verification report for its scope.

## Unity support follow-up

Read the full ticket **#00946121**, including Vaiva's 18 November response (creative approval requires funding) and Sean's 19 November response (consenting-adult GAID option).

Sent a follow-up to growth-support@unity3d.com in the same thread at **10:17 local time, 20 November**:

1. Reported adult-only separate consent and successful SDK ingestion, explicitly distinguishing it from install attribution.
2. Requested manual creative approval or a supported no-spend test route before campaign activation.
3. Asked for the minimum funding requirement if unavoidable, explicitly prohibiting charges or billing changes.
4. Asked whether Internal testing and a Google Play emulator are supported, or a separate physical device is mandatory.
5. Asked whether the Other provider label is expected for the Tenjin applifier endpoint.

No identifiers from a real device, keys or raw logs were sent. No funding, campaign activation, dashboard setting change or synthetic postback was performed.

## Remaining acceptance test

After the creative/test prerequisite is resolved, use a supported separate clean device and the official Unity Ad Testing flow: campaign test QR → ad → CTA → fresh Google Play install → launch → explicit adult ad-measurement consent. Preserve refusal/minor checks. Capture:

- Tenjin attribution to the intended Unity campaign, not Organic;
- a real install callback attempt and successful delivery, with excluded fields audited;
- Unity **Check Install Postback Status** success (documentation allows up to 12 minutes);
- no paid campaign activation and unchanged OPPO save.

If consent timing or the Internal build is not supported by this test route, obtain vendor guidance before changing privacy logic. A manually fabricated click/postback or ordinary app-open success does not satisfy this gate.

## Evidence and references

- [Unity paused / zero spend](unity-paused-zero-spend.jpg)
- [Unity Pending creatives and disabled test](unity-test-blocked.jpg)
- [Tenjin zero callback attempts](tenjin-callback-zero-attempts.jpg)
- [Sent support follow-up](unity-support-sent.jpg)
- [Unity official attribution-link test](https://docs.unity.com/en-us/user-acquisition/data-sharing/attribution/test-attribution-links)
- [Tenjin Unity Ads setup](https://tenjin.com/docs/unity-ads-setup/)
- [Tenjin conversion-test instructions](https://tenjin.com/docs/run-a-conversion-test/)
