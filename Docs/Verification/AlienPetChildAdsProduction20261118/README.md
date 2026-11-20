# Alien Pet 1.0.7 (8) — production bundle handoff

Requested 18 November 2026: build a new production AAB with the existing upload key, upload to Google Play, leave review submission to the owner. Do not activate Unity acquisition campaigns.

## Scope

Production build from the current IOS checkout including the pending child-ad fixes. No gameplay or visual changes for this release task. Package stays com.gravityboxlab.venom. Existing signing setup is used; no replacement key.

ALIEN_PET_CHILD_ADS_QA is removed from the build environment. The production artifact must exclude COgheChildAdsProof and other native benchmark/marketing fixtures. Tenjin stays disabled by the current production resource configuration; no attribution campaign is enabled.

## Protection contract retained

- Android ages 9–12: child-directed, non-personalized banner only on Menu/Home; maximum G content setting. No rewarded/interstitial offers.
- Unknown, under 9 and unresolved legacy Under13: ad eligibility blocked.
- Under 18: Firebase Analytics and Tenjin blocked; adults require explicit analytics consent.
- No AD_ID/AdServices permissions. Official Google App Set dependency retained as approved to avoid native AdMob crashes; this is not an identifier-free SDK claim.
- No saved game data migration or clearing.

## Validation basis

The same child-ad runtime passed 29 privacy/config tests and 16 UI/reward tests on 18 November. Physical OPPO Google Play version 7 then passed two isolated native child-banner runs while preserving level 16, 225 Drops and pet color. See ../AlienPetChildAdsOppo20261118. This release excludes that QA fixture; artifact-specific results are recorded separately here after build.

## Play Console scope

Production currently shows 178 countries/regions (observed during this task), superseding the older US-only documentation. This task does not change country availability. Leave the final Send for review action to the owner; do not submit or roll out automatically.

## Artifact checks passed

- AAB: Builds/AlienPet/GooglePlay/AlienPet-1.0.7-8.aab (58,943,549 bytes).
- SHA-256: e43a4e000f8ecabf1f3030b275fec9cebabaeadf0064b08bf82a08fa779899ba.
- Package/version 1.0.7 (8), ARM64 IL2CPP Release, SDK 26–36, not debuggable or test-only.
- Bundletool validation, JAR signatures/content digests, original upload certificate match and 16KB native/bundle alignment passed.
- Production AdMob IDs including child banner packaged; no AD_ID/AdServices permissions.
- COgheChildAdsProof and listed benchmark/marketing diagnostic types absent from IL2CPP metadata.
- Exact-build symbol ZIP prepared and verified by Build IDs plus code bytes for seven libraries; four third-party libraries have no verified symbols available. R8 is disabled, so there is no mapping.txt.
- This task has not installed the new AAB on a device; real OPPO runtime results were from version 7 with the same protective runtime and an isolated test fixture.

## Upload completed

Google Play accepted version 8 (1.0.7) plus exact-build native symbols. Release name: **1.0.7 — Child-safe ads and stability**. Saved in Production, then verified under **Changes not yet submitted for review** in Publishing overview. The owner can press **Submit 1 change for review**. No review submission or campaign activation was performed.

Publishing URL: https://play.google.com/console/u/0/developers/5637064096940148363/app/4974079895895643247/publishing

Quick checks were still running at handoff (UI estimated up to 14 minutes). Managed publishing remains off: after the owner submits and Google approves, this saved full-rollout update can publish automatically. Current country availability is unchanged.

Two nonblocking warnings remain: the preexisting Advertising ID declaration says Yes while the artifacts omit AD_ID (release-blocking was already disabled), and no R8 mapping file because minification is disabled. No AD_ID permission was added to silence the warning. No additional declarations or app-content claims were changed in this task.

Screenshot: publishing-ready.png. Supported phone/tablet device counts are unchanged: 8,426 phones and 4,797 tablets.
