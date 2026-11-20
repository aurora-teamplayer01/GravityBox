# Tenjin intentional identifier opt-out — 18 November 2026

Status: **Tenjin ingestion verified on OPPO through Google Play Internal version 6.** Live Unity paid-install attribution is not verified and campaign remains disabled.

## Scope

Follow Tenjin support guidance supplied by the user. Preserve OPPO game progress, retain adult explicit Analytics consent and all child protections, do not enable the Unity campaign or promote to Production. Updating through Play is necessary because the installed app uses Play App Signing.

## Changes

- Replace OptInParams with explicit OptOutParams before every connect; withhold advertising ID, developer device ID/scope, IP/fingerprint/device metadata and other excluded fields. Retain Play referrer and required SDK installation/session/event fields.
- Embed the existing pinned official Unity SDK 1.23.2 / Android 2.0.2 under Packages/com.tenjin.sdk. Remove its Google play-services-appset dependency; root Gradle exclusion prevents another plugin reintroducing that module.
- Disable both Probabilistic Modelling windows in Tenjin app Attribution. Unity Ads inherits those Off values; its override remains disabled. Screenshot: attribution-off.jpg.
- Existing unknown/minor/declined gating is unchanged. Repository androidEnabled remains false; only the Internal test artifact enables the integration.

## Verification before upload

- 22/22 focused Unity EditMode privacy/attribution tests passed.
- Signed ARM64 IL2CPP AAB 1.0.5-tenjin-test (6), existing upload certificate verified. Bundletool, signed content digests and native 16 KB alignment pass.
- No AD_ID or AdServices permission in final manifest.
- No com.google.android.gms.appset class definitions in final DEX. Optional references remain in other SDKs; string references alone do not imply the provider is bundled. AndroidX/Tenjin/Unity helper classes remain, so absence of Google's provider is not a claim that every framework helper was removed.
- Gradle releaseRuntimeClasspath dependencyInsight confirms no play-services-appset dependency.
- Packaged Tenjin test config enabled and key matches local configuration; no key included in evidence. Repository config restored disabled.

## Limits

Successful event ingestion would not prove paid Unity install attribution. Tenjin support states live Unity clicks are S2S and need GAID or IP matching under its current integration; our restrictions remain, so those live installs would be organic. This update preserves the original Play referrer and cannot demonstrate a fresh attributed install. Do not uninstall or clear OPPO data to force attribution.

No campaign activation, production rollout, commit or push in this task.

## Internal release

Published version 6 to the existing Internal testing track at 09:18 Asia/Ho_Chi_Minh on 18 November. Console confirms Available to internal testers. Matching native symbols uploaded; R8 is disabled so no mapping file exists. Existing AD_ID declaration warning remains; no permission was added and no policy declaration was changed. Device support unchanged. See internal-release.jpg.

## Real OPPO verification

- Updated in place from Google Play, version 1.0.5-tenjin-test (6), installer com.android.vending. firstInstallTime remains 2 November; no uninstall or data clearing. Before playing: level 14, 320 Drops, mint pet preserved.
- Before adult Analytics opt-in, startup log contains no Tenjin initialization. User explicitly enabled Analytics in Privacy and played a level.
- Native Tenjin diagnostics: AppSet library not available (ClassNotFoundException). This is the expected optional-provider check, not a game crash. No successful App Set ID fetch.
- Connect at 09:21:36 received code 200 / success true, with nonempty Play referrer and explicit opt_out_params. Inspected payloads exclude advertising_id, developer_device_id/scope and ip_address. Only SDK/app/session/referrer fields and bounded level event/value are present.
- Accepted level_start=14 (09:21:37), level_complete=14 (09:21:54), level_start=15 (09:21:55), each code 200 / success true. No code 202 in this run.
- Tenjin SDK Integration Status for 1.0.5-tenjin-test now shows **App Open: received; Referrer Install: received; Custom Event: received**. Screenshot tenjin-received.jpg. Ad Mediation Impression and IAP remain not received; this task does not forward those events to Tenjin and does not claim otherwise.
- Referrer Install received is ingestion evidence; it is not a new reinstall, paid Unity-attributed install, or verified Unity conversion/postback. Do not enable the paid campaign on this evidence alone.
- Native evidence is sanitized; no SDK key, actual referrer or device/installation identifiers are retained in report logs.

Consent restoration: user switched Analytics Off; independently reopened OPPO Privacy and visually verified “Analytics: Off · turn on”, with age unchanged at 18 or older. No Tenjin lines appeared in the available log buffer; this limited observation is not a complete network traffic audit. The 22 passing tests also cover gating/revocation/regrant. No broader age or data collection permissions were introduced.
