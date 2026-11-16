# iOS services configured on branch IOS — 16 November 2026

Use branch **IOS** from `git@github.com:aurora-teamplayer01/GravityBox.git`. Do not recreate these apps or reuse Android ad units. The public bundle ID stays `com.gravityboxlab.venom` for both Apple devices.

| Service | Actual configuration |
|---|---|
| AdMob iOS app | `ca-app-pub-7806638519709442~8888402391` |
| Banner | `ca-app-pub-7806638519709442/1433739156` — AlienPet_iOS_Banner |
| Interstitial | `ca-app-pub-7806638519709442/1349978676` — AlienPet_iOS_Interstitial |
| Rewarded | `ca-app-pub-7806638519709442/5181412470` — AlienPet_iOS_Rewarded |
| Firebase project | `coghe-57f21` / `446362971270` |
| Firebase iOS app | `1:446362971270:ios:8d705f7bc5e2ebbddc3507` |
| Config in source | `Assets/GoogleService-Info.plist` (mobile client config, not an admin/service-account credential) |
| Analytics property | `557269271`, existing account `58477001` |
| iOS stream | `16348097813` — Alien Pet iOS |
| AdMob ↔ Firebase | Linked successfully in AdMob; app selection verified as Alien Pet iOS |
| European consent | **Alien Pet - iOS - European consent**, English, Published; Do not consent enabled for all regions; uses UMP, not ad-unit fallback deployment |
| US privacy | **Alien Pet - iOS - US privacy**, en-US, Published; all current/future supported US states |
| Policy URL | `https://cj.suri.house/alien-pet-privacy` |

Console account used: the existing authorized Google account, kienphamds@gmail.com. No new account, payment method, IAM role, service-account key or Apple credential was created. Dashboard links:

- [AdMob iOS units](https://admob.google.com/v2/apps/8888402391/adunits/list)
- [Firebase iOS app](https://console.firebase.google.com/project/coghe-57f21/settings/general/ios:com.gravityboxlab.venom)
- [iOS Analytics stream](https://analytics.google.com/analytics/web/#/a58477001p557269271/admin/streams/table/16348097813)

## Runtime and privacy contract

`COgheGoogleServices.Boot` supports Android and iOS only in a native player, with the correct bundle. `COgheAdMob` selects platform-specific production **and** Google test unit IDs at compile time. Existing placements, caps, rewards and economy are unchanged. Debug or QA builds use test units; `COGHE_STORE=1` exports without test tools. Test ads never establish real income or production fill.

Unknown/under13: neither Ads nor Analytics initializes. Teens 13–17: no gameplay Analytics; existing teen/under-consent-age flags, G-rated ads and disabled personalization remain. Adults: explicit Analytics choice and independent UMP consent. Withdrawal suspends providers and discards stale async permits. Remote Config remains adult Analytics opt-in only.

The iOS postprocessor closes Firebase collection/Analytics storage/ad storage/ad user data/ad personalization/IDFA/IDFV by default before managed startup, delays GMA measurement, and disables Analytics registration with ad networks. This release does **not** request ATT; no IDFA explainer is published. Do not add an ATT prompt to children or infer an App Privacy legal answer solely from these switches. SDK manifests and actual data use still require archive review.

AdMob rewarded unit uses the generic `1 Reward` setting because the game grants the placement's advertised benefit from the earned callback, not the dashboard quantity. Existing code grants once and logs ad lifecycle/revenue only through the existing consent-gated Analytics sink. The linked dashboards do not opt users into Analytics.

## What requires the receiving Mac / App Store identity

1. Select an Apple distribution team that owns `com.gravityboxlab.venom`. Register App Store Connect app if not present. Add the resulting numeric App Store ID (and actual Apple Team ID where relevant) to Firebase settings. These IDs cannot be invented here.
2. Once the iOS listing exists, attach it to the AdMob iOS app and complete its app readiness/app-ads.txt checks. AdMob currently correctly reports **Requires review / limited serving** for the unlisted app. No-fill before listing/review is not evidence of a code defect.
3. Validate signing/archive and run TestFlight on both iPhone and iPad. Test privacy age matrix, UMP accept/deny/reopen, rewarded earned/early close/no-fill, interstitial, pause/resume and reward persistence.
4. On a **test-ad build**, opt an adult test fixture into Analytics; launch from Xcode with `-FIRDebugEnabled`, win/lose a level and complete a rewarded test ad. Check iOS stream/DebugView `level_complete`, `level_fail`, `rewarded_earned`, `ad_show`, `ad_revenue`. Remove debug launch arguments for release. Do not fabricate revenue or click production ads to create evidence.
5. Paid events can report zero/unknown values for test ads. Production dashboards may be delayed; confirm real revenue only after legitimate serving. Adult Analytics refusal and under13/teen tests must not emit gameplay telemetry.

Test evidence and exact limitations are recorded in `Docs/Verification/AlienPetIOSServices20261116/README.md`. Firebase app/stream existence and static tests do not prove on-device event delivery. No signing keys or credentials are part of Git or the handoff ZIP.
