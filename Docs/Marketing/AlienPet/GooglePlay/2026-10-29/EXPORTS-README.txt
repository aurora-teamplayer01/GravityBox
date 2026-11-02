ALIEN PET: ESCAPE PUZZLE — GOOGLE PLAY VISUAL PACK
29 October 2026

UPLOAD FILES
  exports/app-icon-512.png       512 x 512, opaque RGB PNG; approved icon C
  exports/feature-graphic.png   1024 x 500, opaque RGB PNG; illustrative key art
  exports/phone/*.png           Six 1080 x 1920 opaque PNGs, numbered in order
  exports/tablet-7in/*.png      Actual native tablet compositions when available
  exports/tablet-10in/*.png     Separate actual native tablet profile compositions

REVIEW
  screenshot-review.html                    Local full pack gallery
  qa/screenshot-contact-sheet.png           Shareable visual proof sheet
  qa/screenshot-render-audit.json            Dimensions, source hashes, crop and layout checks
  sources/native-still-audit.json            Original screenshot sources and exact video timestamps
  listing-copy.en-US.json                    English title, short/full copy, character counts and factual evidence

REPRODUCE
  node render-screenshots.cjs
Run from this folder or call the script with its absolute path. The script uses
the bundled Codex Node libraries and the installed local headless Chromium.
It opens only these local generated HTML files; no user browser profile is used.
Optional environment paths: ALIEN_PET_NODE_MODULES, ALIEN_PET_CHROMIUM.

HOW THE PHONE SCREENSHOTS WERE COMPOSED
Actual Unity/native source images form the large image area. HTML/CSS adds
headlines and the approved ivory/teal styling. Source images are uniformly scaled
and cropped only by rectangles recorded in screenshots.config.json. No scene,
creature, color, accessory, puzzle geometry or game UI was retouched or redrawn.
The source screen margins and outer controls excluded by each crop are documented.
The original files remain in sources/. The glass enclosure is preserved in full.
The Boss 10 original is an actual Unity scene render from the existing approved
verification folder; its fixture paused gameplay ticking and had no HUD. It does
not prove a completed solve or mobile performance.

TABLET PROVENANCE
Tablet inputs must be real native tablet captures. The pipeline keeps each whole
tablet frame, contain-fit into a 1440 x 2560 marketing canvas. Phone images are
never upscaled and relabelled as tablet. The 7-inch-equivalent sources use a
native 1200 x 1920 Android tablet profile; 10-inch-equivalent sources use a
native 1600 x 2560 profile. These are emulator captures, not evidence of
separately tested physical 7-inch and 10-inch devices.

ICON AND FEATURE ART
The approved text-free icon C is retained whole, mechanically resized to 512 x
512. No text, crop, rounded border or new icon treatment is added.
The root-approved illustrative feature-art master is mechanically resized whole
to 1024 x 500. It is presented as feature artwork, not an actual gameplay capture.
Source prompt/provenance is recorded separately by the coordinating agent.

CHECKS
The export script checks native input dimensions, crop boundaries, title
overflow, header/image/footer separation, output dimensions and lack of alpha.
Each output and input gets a SHA-256 hash. Visual QA includes the contact sheet,
full-size puzzle, customization, icon and feature images. English copy lengths
are 24 / 70 / 1289 characters (title / short / full) at the initial copy draft.

These are prepared final marketing assets. Google Play upload, submission and
publication status are tracked separately in the Console status record.
