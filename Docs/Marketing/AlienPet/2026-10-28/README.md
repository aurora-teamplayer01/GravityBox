# Alien Pet: Escape Puzzle — mixed-media ads & resource kit

28 October 2026. Three new English vertical ads (18–25 seconds), plus five rebranded historical exports. The user approved AI credit usage and the public game name. COghe is the creature's name.

## Deliverables

- **Escape Artist — 22 seconds:** a cheeky monster-head hook, cinematic lever action, actual switch operation and crate movement, liquid pipe escape, and the new title/Play Now end card.
- **Your New Alien Friend — 24 seconds:** a scientist meets COghe, actual Home affection, the monster-head surprise, real puzzle actions, and a warm character payoff.

- **Tiny Pet, Big Personality — 24 seconds:** cute feeding hook, actual apple-pie/chilli reactions, Mint injection, flower/party/bee hats and affection. A pet-care-focused creative with playful original marimba/pluck music.

Watch all eight in [review.html](review.html). Final files are in [videos](videos/).

## Source and continuity

- Flow project: https://flow.google.com/project/47cd8aa7-9895-4d44-90ce-86b452bf8504
- Veo 3.1 Quality: four 8-second shots from start frames using the existing creature, scientist and lab reference masters. One 8-second pipe replacement was generated because the first version changed the creature's appearance. Approved video-generation charges shown in Flow: 400 + 100 credits. Image-generation usage is not included in that figure.
- The original pipe video is excluded from both edits. The repaired pipe shot is trimmed before its later off-model facial features; the end card returns to the actual in-game creature.
- Actual gameplay was captured from this branch's native Unity Product player at 30 fps with 1080×1920 screenshots. Level 4 and 6 were solved using normal touch commands and runtime physics; the capture fixture never teleports puzzle objects. Home uses an unlocked in-memory fixture. No player progress or purchases were saved.
- The montage crops and, for some actions, speeds up genuine footage (1.1× or 1.4×). End-card idle is slowed for readability, with a clean graphic matte masking cropped background/UI fragments away from the creature. Scene labels distinguish **AI CINEMATIC** from **ACTUAL GAMEPLAY**. AI footage is a dramatization, not a claim about runtime graphics or an exact level layout.
- Original synthesized music, game sound effects and native generated foley/dialogue; no third-party commercial song or celebrity likeness.

## Edit scripts and sources

Working folder: `Artifacts/AlienPetAds-2026-10-28/` (local, ignored by Git).

- [edit_ads.py](edit_ads.py): reproducible ffmpeg/Pillow edit and original score synthesis (uses sources in the local working folder).
- [verify_ads.py](verify_ads.py): media validation and contact sheets.
- `sources/`: five AI source videos and the encoded native gameplay reel.
- `references/`: the three continuity masters.
- `edit/*-manifest.json`: source trims, time remapping and timeline placements.
- `gameplay-frames/marks.txt`: runtime operation/solve timestamps, indexed in 30 fps frames.

Source video names/ordering: shot 01 = lever; shot 02 = rejected pipe; shot 03 = first contact; shot 04 = monster head; shot 02R = repaired pipe. Flow's original collection contains the first four; the repair is a separate asset in the same project.

## Verification

Final A and B MP4s are H.264, 1080×1920, 30 fps, stereo AAC 48 kHz, web fast-start. Durations are 22.00 / 24.00 seconds (660 / 720 frames). The AI source downloads are native 720×1280/24 fps, scaled for the final edit; this is not native 1080p AI generation. No detected black intervals ≥0.1 s; audio peaks remain below 0 dBFS. Shot contact sheets, end-card readability and the branding were reviewed. See [qa/validation.json](qa/validation.json).

These are local review exports; no campaign has been published. Evaluate audience response with an A/B test before claiming CPI or retention improvement.

## Cute pet-care addition and complete resource kit

Clip C sources and reproducible edit are in `Artifacts/AlienPetPetAd-2026-10-28/`. Two additional 8-second Veo 3.1 Quality shots were generated using start frames and the same masters (200 video-generation credits approved; image charges excluded). Collection: **Alien Pet - Cute Pet Care**. The existing cinematic collection remains available.

The Unity capture completed with two actual snacks consumed and the real Mint hold/hats/petting interactions. It uses an in-memory unlock/ownership fixture and writes no player saves. 1,135 native 1080×1920 frames were encoded at 30fps; the new ad is 720 frames / 24 seconds. Source-trim mappings are in [C-edit-manifest.json](C-edit-manifest.json). Crop mattes hide only clipped UI shadows, away from the creature. End-card footage is slowed to 0.65×.

C passed full decode, frame count, format, black-frame and audio checks; [C validation](qa/C-validation.json). The source AI videos remain 720p, upscaled for delivery.

Resource folder: `Docs/Marketing/AlienPet/AdsResources/`. ZIP: `/Users/mrk/GravityBox/Docs/Marketing/AlienPet/AdsResources/Packages/Archive/AlienPet_Ads_ResourceKit_2026-10-28.zip`. Includes 3 current-brand ads, 4 historical COghe ads and 2 labelled reference exports, thumbnails, English ad copy, metadata/checksums, native C source reel, two C AI shots and its edit script. The index is portable within the ZIP. The silent reference is explicitly not upload-ready. Historical COghe files need rebranding before use.

[Resource kit gallery](../AdsResources/index.html) · [Packaging script](package_ads.py). Nothing has been posted to an ad network.

## Historical ads rebranded — revision 2

Five historical exports now use **Alien Pet: Escape Puzzle**:

| New ID | Original | Duration |
|---|---|---|
| D | OLD01 puzzle | 15s |
| E | OLD02 pet | 17s |
| F | OLD03 showcase | 30s |
| G | OLD04 cinematic trailer | 30s |
| H | REF01 alternate gameplay tour | 30s |

The native video caption bands and full end cards were rebuilt with the current Manrope/ivory/teal identity and approved text-free app icon C. Obsolete “60 puzzles” claims and “PLAY FREE” were removed. Original gameplay, timings and audio remain; audio levels were normalized. G was rebuilt from the original unbranded picture-lock and final WAV mix, so no old wordmark is painted over. G is labelled AI CINEMATIC; D/E/F/H are actual historical gameplay. COghe is retained where it names the creature in actual UI, such as “What does COghe want?”. No source export was overwritten.

All five are 1080×1920, 30fps, stereo AAC 48kHz and passed full decode, exact frame-count, black-interval and audio-peak checks. See [rebrand validation](qa/rebrand-validation.json), [source mapping](rebrand-manifest.json) and [edit script](rebrand_ads.py). No new Flow credits were needed.

**Latest full kit:** `/Users/mrk/GravityBox/Docs/Marketing/AlienPet/AdsResources/Packages/AlienPet_Ads_ResourceKit_2026-10-29_scene2fix.zip` (8 current-brand ads + original archive/reference exports and sources). **Upload selection kit:** `/Users/mrk/GravityBox/Docs/Marketing/AlienPet/AdsResources/Packages/AlienPet_Ads_8Clips_2026-10-29_scene2fix.zip` (only the 8 Alien Pet ads, thumbnails, copy, metadata and gallery). The previous ZIP is preserved. The silent REF02 remains a source reel, not a finished ad requiring a wordmark replacement.


## Scene 2 correction — 29 October 2026

Cảnh 2 đã sửa 29/10: COghe bò ở mặt trong kính; giữ nguyên nhạc, lời thoại và các cảnh khác.

The replaced interval is frames [241, 331) at 30fps (8.033333–11.033333s). The 30s final retains its original AAC packets, including timestamps. Flow uses a corrected start frame and the same character/scientist references. Original exports and prior ZIPs are preserved.
