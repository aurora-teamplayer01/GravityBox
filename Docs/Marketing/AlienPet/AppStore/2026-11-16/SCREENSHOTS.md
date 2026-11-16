# Screenshot production and replacement

## Assets supplied

- `assets/icon/AppIcon-1024.png`: approved C, 1024 × 1024, opaque RGB PNG, square canvas; no added corners/text. Source included. Ready as artwork; final archive must use it.
- `screenshots/draft/iphone`: six designed layouts, 1320 × 2868.
- `screenshots/draft/ipad`: four designed layouts, 2064 × 2752.
- `sources`: unchanged source captures and icon. `screenshots.json` records original path, platform and crop. Android/Unity captures are reference artwork, **not iOS evidence**. Watermark intentionally prevents accidental submission.

Apple accepts these portrait dimensions in the large iPhone and 13-inch iPad groups. Check current Console slots and specifications on upload day. The package deliberately does not create fake iPhone hardware frames or stretch phone UI into an iPad capture.

## Capture list

| Order | iPhone | iPad | Scene/action |
|---|---|---|---|
| 1 | Tiny pet, big escape | Tap, think, find a way | Current display 4: creature, switch, trace and exit visible |
| 2 | Move it, reveal the way | Move it, reveal the way | Display 6: move red crate enough to reveal floor exit |
| 3 | Think beyond the floor | Small parts, big ideas | Phone Boss10 lift/bridge; iPad display29 splitter/tube, confirm current mapping |
| 4 | Snack time | New paths | Phone Home feeding apple; iPad display34 swing/tube |
| 5 | A splash of personality | — | Home Style, Mint ink applied; keep actual selection UI |
| 6 | Make it your own | — | Home Style, colorful flower crown |

If a scene changed after source commit, capture the current truthful mechanic and revise caption. No debug selector, developer overlay, unrelated OS notification, live advertising, personal name or device identifier in screenshots. Use a legitimate completed test save; do not imply all cosmetics unlock at first launch. Never alter the physics/layout solely for an impossible marketing scene.

## Capture on the receiving Mac

Use real iPhone/iPad screenshots via Xcode Devices, or a **Spatial** simulator build from the same release source. The existing generic simulator script targets the wrong prototype; see IOS-GAPS.md. Boot the chosen device; navigate the normal app to the desired view. For a simulator screenshot:

```sh
export DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer
xcrun simctl list devices available
# Use the UUID of the specific BOOTED simulator, not a guessed UUID.
xcrun simctl io "$SIMULATOR_UDID" screenshot /absolute/path/shot.png
```

Use an iPhone large display (for example 17 Pro Max) and iPad Pro 13-inch separately. Keep native PNG; source may differ from target marketing canvas because renderer contains/scales uniformly. Inspect the full iOS screen at native resolution. iOS appearance and ad/privacy flows still need physical-device QA even if simulator captures are used.

For each row in `screenshots.json`:
1. Replace `source` with the new relative PNG path inside this folder.
2. Set actual `sourceWidth` and `sourceHeight`.
3. Set `crop` to null initially (whole screenshot); if needed crop only empty margins with explicit rectangle, never relevant gameplay/UI. Recheck headline spacing.
4. Set `provenance` to `iOS device` or `iOS simulator`, `iosVerified` to true **only after visual comparison**, and fill `buildVersion`, `deviceModel`, `captureDate`. Keep original reference record in version history.
5. Record final source commit and test build separately in the QA sheet.

## Reproduce layouts

Requires Node.js and Python3. All fonts/images are included; no external image requests.

```sh
cd /path/to/2026-11-16
npm ci
npx playwright install chromium
node tools/render.cjs          # draft, retains watermark
node tools/render.cjs --final  # blocks unless every row has verified iOS provenance
python3 tools/verify-package.py --assets
```

Final output goes to `screenshots/upload/iphone` and `screenshots/upload/ipad`. Check each image at 100%, then use `qa/contact-sheet.png` and `review.html`. Never put draft images into Media Manager. Keep at least the first three screenshots focused on actual gameplay. Do not use the live-action/AI advertising clips as App Preview; a native gameplay preview is optional and not included in this first package.

Font files are copied from the existing approved marketing kit; `design-assets/FONT-LICENSE.txt` includes the font license. No new character illustration or game art was invented.

Reference: https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/
