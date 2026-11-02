# Alien Pet: Escape Puzzle — rebrand, 28 October 2026

The user approved **Alien Pet: Escape Puzzle** as the game name. **COghe** remains the creature's name. The approved text-free icon and Glass C art remain unchanged.

## Implemented

- Main Menu: a two-line Alien Pet / Escape Puzzle wordmark in the existing Manrope and teal palette, above the real 3D creature.
- Upgrade: Alien Pet Plus, including the offer and owned message. Test purchases still explicitly say no real payment.
- Android and iOS builders: full new app display name. Existing package/bundle ID, SKU IDs, analytics project, resource paths and saves remain unchanged.
- Product/Spatial Mac builds: CFBundleDisplayName and CFBundleName updated after build; Unity's internal product name retained to keep existing desktop PlayerPrefs and save path.
- Intro, Understand COghe, Home and tutorial references retain the character name. No scene, mechanism, progression or reward changes.

## Verification

- 6 focused PlayMode checks passed: real menu creature and greeting; Pause input isolation; fresh launch intro/skip; full intro handoff; returning-player progress; Plus offer gating.
- Native macOS Product build succeeded. Three actual window ratios captured: 540×960, 540×1170, 768×1024. Title width fits at each size. Plus, gameplay and Pause captured.
- The Google Mobile Ads editor plugin logs its pre-existing Mac Info.plist path exception during postprocessing; Unity still reports success and the native player runs. The final Contents/Info.plist passes plutil validation.
- Android/iOS naming source changed; no new device install is claimed in this task.

Reproduce captures with the development player argument `-alien-pet-brand-proof <absolute output folder>`. This fixture uses in-memory progress and a test-only intro override. Gallery: [review.html](review.html).
