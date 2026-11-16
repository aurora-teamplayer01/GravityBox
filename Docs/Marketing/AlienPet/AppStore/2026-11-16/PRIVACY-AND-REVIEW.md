# Privacy and App Review handoff

## Status and release identity

This is a worksheet, not a submitted privacy declaration or legal certification. Audience previously chosen by the owner is 9–15; age rating and Kids Category are separate decisions. Keep under-13 protections and truthful fantasy/monster disclosures. Do not copy Google Play's IARC rating into Apple's questionnaire.

The current source has no account login in its normal game flow. Check the final iOS build before declaring that reviewer login is unnecessary. No social/chat/UGC feature is claimed by this package; audit the submitted build if these change.

## Age and consent matrix

| Player state | Analytics | Ads | Verification |
|---|---|---|---|
| Unknown age | Off | Off | Fresh install, no SDK auto-init/network leakage attributable to those services |
| Under 13 | Off | Off | Relaunch and progression still playable; no ad-dependent lock |
| 13–15 or 16–17 | Off | Restricted, only if legally eligible and consent permits | iOS UMP/age tags/ad content and denial path verified |
| Adult, before Analytics choice | Off | Wait for valid setup/consent | No premature ID/analytics collection |
| Adult, declined Analytics | Off | Separate consent-dependent ads | Analytics refusal does not silently consent to tracking |
| Adult, opted in | Allowed | Separate consent-dependent ads | Correct iOS stream and events; withdrawal persists |

Existing C# age bands are not proof that native Pods respect startup rules. Audit Info.plist defaults, Firebase auto-init, adapters and any remote settings before capturing native evidence.

## App Privacy worksheet

Collect the final SDK inventory and Xcode privacy report. For each data type record whether collected off-device, purpose, linked to identity, tracking, retention and deletion path. Include SDK behavior even when app code does not directly send it. Optional adult Analytics does not automatically mean the app qualifies for “Data Not Collected”.

| Area to investigate | Evidence required before answer |
|---|---|
| Identifiers | Firebase app instance/installation identifiers; any advertising or vendor identifiers actually used |
| Usage and advertising data | Level start/win/loss, ad impression/reward/revenue, ad interaction and personalization settings |
| Diagnostics | Actual enabled SDK crash/performance/error reporting; do not declare Crashlytics installed based on generic Firebase presence |
| Purchases | StoreKit/product transaction handling if included; receipt/server path and retention |
| Contact information | Support communications are outside the game's local save; disclose when the Apple questionnaire requires |
| Local data | Progress, style and age choices remain local unless final build changes that; do not claim optional Analytics never leaves device |

Tracking is based on use and sharing, not solely IDFA access. ATT, UMP and the app's Analytics choice are distinct. Never promise “no tracking” until SDK data use and the final archive support it.

## URLs and support

Existing approved contact: `support@suri.house`.
Existing policy: https://cj.suri.house/alien-pet-privacy
Existing website: https://cj.suri.house

The automated HTTP check on 16/11/2026 returned 403 for both URLs. This may be bot protection, not proof the website is down. **Release gate:** verify externally in Safari without login, from iOS and the intended storefront region. Resolve challenges/403 if reviewers or users cannot reach it. See `qa/website-check.json`.

The policy was previously written for the Android release. Update its platform/SDK/IAP language for the actual iOS integration before submit. Do not claim this handoff redeployed it. The draft support URL points to the policy contact/deletion section because it contains the existing support route; a dedicated Alien Pet support page with email and FAQ is recommended. The optional marketing URL is deliberately blank in metadata because the existing homepage brands Critter Jam; fill only with a genuine Alien Pet page. Do not submit misleading game branding.

No automatic account deletion flow is claimed. The current game has local progress and no player login; if accounts are added, revisit account deletion requirements. Turning Analytics off does not erase historical provider records.

## Content and rights

Retain the approved icon C and game assets. The creature has a temporary monster-head act with teeth/tongue and fantasy puzzle situations; review the full game and cinematic intro when answering fear/violence questions. Do not use Venom/Marvel as public metadata, keywords or licensing claims. The unchanged historical bundle identifier is a technical ID. The owner must confirm rights to all artwork, AI-generated assets, audio and fonts; this package includes the Manrope license but is not an IP clearance.

## Reviewer notes

Use `metadata/en-US/review-notes.template.txt` only after filling tested build details. Provide real review contact name/email/phone from the Apple account owner. No credentials are included. Explain progressive unlocks and where to find Privacy. Do not create a hidden reviewer-only gameplay mode or promise debug level access in production.

App Preview is optional: omit for first submission unless a truthful native gameplay capture is prepared. The existing Hollywood/AI ads are marketing materials, not representative App Preview footage.

References:
- https://developer.apple.com/app-store/app-privacy-details/
- https://developer.apple.com/app-store/review/guidelines/
- https://developer.apple.com/documentation/apptrackingtransparency
