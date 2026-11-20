# Alien Pet: Escape Puzzle — App Store submission draft (1.0.0)

Draft 16/11/2026 for owner review. Nothing below has been entered in App Store Connect except the items marked **[DONE]**.

## Already in App Store Connect

| Item | Value |
|---|---|
| App | Alien Pet: Escape Puzzle — Apple ID `6835357287` **[DONE]** (name fixed from the bundle ID) |
| Bundle ID | `com.gravityboxlab.venom`, team Bui Ngoc Quang `RPLRU9Q87A` **[DONE]** |
| SKU | `com.gravityboxlab.venom` (internal, cannot change) |
| Build | 1.0.0 (1), processing **VALID** **[DONE]** |
| IAP | `coghe_plus` $5.99, `coghe_no_ads` $2.99, Non-Consumable, en-US, all territories **[DONE]** — review screenshot missing |

## 1. App Information

| Field | Proposed value |
|---|---|
| Name | Alien Pet: Escape Puzzle |
| Subtitle (≤30) | Squishy friend. Clever escapes |
| Primary category | Games → Puzzle |
| Secondary category | Games → Adventure |
| Content rights | Does not contain, show or access third-party content |
| Age rating | see §4 → expected **9+** |
| Copyright | 2026 YoloStudioGlobal |

## 2. Version 1.0.0 — Store listing (en-US)

**Promotional text (≤170)**
Meet your little alien friend. Climb glass, move mechanisms and discover clever escapes. Then make time for snacks, colors and a little personality.

**Description** — `metadata/en-US/description.txt` (1,290 characters), unchanged. Optional last line since IAP is real: "Optional one-time purchases: Alien Pet Plus and No Ads."

**Keywords (≤100)**
logic,brain,teaser,physics,maze,lab,creature,virtual,companion,casual,adventure,customize

**Support URL**: https://cj.suri.house/alien-pet-privacy#deletion
**Marketing URL**: (blank)
**Privacy Policy URL**: https://cj.suri.house/alien-pet-privacy — returns 200 to an iPhone Safari user agent, **but the text is Android-only and says there are no real-money purchases. Must be updated before submit.**

**Screenshots**: required 6.9" iPhone (1320×2868) and 13" iPad (2064×2752). The current set is a watermarked draft from Android/Unity captures; it cannot be uploaded. Native iOS captures needed.

## 3. App Privacy (nutrition label) — proposed answers

Data collected: **Yes**. Tracking: **No** (no ATT prompt; ads are contextual/UMP; Analytics is adult opt-in).

| Data type | Collected by | Purpose | Linked to user | Tracking |
|---|---|---|---|---|
| Identifiers → Device ID | AdMob, Firebase (app instance ID) | Third-party advertising, Analytics | No | No |
| Usage Data → Product Interaction | Firebase Analytics (adults who opt in) | Analytics | No | No |
| Usage Data → Advertising Data | AdMob | Third-party advertising | No | No |
| Diagnostics → Other Diagnostic Data | AdMob SDK | Third-party advertising | No | No |
| Location → Coarse Location | AdMob (IP-derived) | Third-party advertising | No | No |
| Purchases → Purchase History | `iap_*` events via Firebase (adults who opt in) | Analytics | No | No |

Not collected: contact info, health, financial info, precise location, user content, browsing/search history, contacts. Verify against the Xcode privacy report of the archive before submitting.

## 4. Age rating questionnaire — proposed

| Question | Answer |
|---|---|
| Cartoon or fantasy violence | Infrequent/Mild (fantasy monster-head animation, puzzle peril) |
| Realistic violence, sexual content, nudity, profanity, drugs, alcohol, tobacco, gambling, horror/fear | None (monster act has no scare and no voice) |
| Medical treatment information | None |
| Unrestricted web access | No |
| User-generated content / messaging | No |
| Contests | No |
| Loot boxes | No (mystery gift box is earned in levels, not bought) |
| In-app controls (parental / age assurance) | Yes, in-app age gate |
| Advertising | Yes |

Expected result: **9+**. Not in the Kids Category (it has third-party ads).

## 5. Pricing and availability

Free, base country United States. Availability: proposed US first, as in the handoff, or all countries (owner choice). Release: **manual** after approval.

## 6. App Review information

**Sign-in required**: No.
**Contact**: [owner fills: first name, last name, phone, email]

**Notes (draft, verified against the build where marked):**

Alien Pet: Escape Puzzle is a single-player 3D puzzle game. Players guide COghe, a liquid alien, through glass laboratory rooms by tapping reachable surfaces and devices. No account or login is required.

First launch: the game asks for an age range before any ads or Analytics. Players under 13 or who skip the question get no ads and no Analytics. Teens get restricted ads and no gameplay Analytics. Adults can opt in to Analytics. Settings: Pause → Privacy.

Home, Style (colors and accessories) and the shop unlock after the first Boss level (level 10).

In-app purchases (non-consumable): Alien Pet Plus (no ads, rewards without watching ads, double Drops) and No Ads. They are shown in the Plus popup, reached from the Plus button on the main menu **once Home is unlocked (after level 10)**, with Restore Purchases at the bottom of the popup. Drops are a virtual currency earned in play and are not sold.

Ads: banner on the menu, Home and victory screen; an interstitial at most every second win from the 6th win; optional rewarded ads for hints, items, Drops, the daily gift and tripling a win reward. None for under-13 or unknown age.

The creature can briefly turn into a cartoon liquid "monster" as a playful idle animation; it has no scary content.

Build 1.0.0 (1). [Tested on iPhone __ / iPad __ through TestFlight on __.]

## 7. Export compliance

Uses encryption: only HTTPS through the OS and Google SDKs → **exempt**. Recommend adding `ITSAppUsesNonExemptEncryption = NO` to Info.plist in the next build so TestFlight does not ask every time.

## Blockers before Submit for Review

1. **Privacy policy**: rewrite for iOS (App Store, StoreKit purchases, AdMob/Firebase on iOS). It currently says Google Play only and "no real-money purchases" — a likely rejection (5.1.1) and an inaccurate statement.
2. **IAP reachable by review**: either explain the path to level 10 (above) or show Plus/Restore earlier (e.g. Pause menu from level 1). Apple often rejects with 2.1 when reviewers cannot find the IAP.
3. **Native iOS screenshots** (iPhone 6.9", iPad 13") and **one review screenshot per IAP**.
4. **Device QA on TestFlight**: age matrix, sandbox purchase/restore/refund, ads.
5. **Review contact** details.
