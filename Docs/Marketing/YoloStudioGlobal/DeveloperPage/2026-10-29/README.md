# YoloStudioGlobal — Google Play developer page

Artwork and bilingual brand copy prepared on 29 October 2026. **Saved in Google Play Console:** it confirmed “Your changes have been saved.” The public developer collection still showed its previous app-list layout at the latest check, so **public propagation remains pending**. No legal identity or account-profile information is changed by this pack.

## Upload assets

| File | Dimensions | Format | Bytes |
| --- | --- | --- | --- |
| [Developer icon](exports/developer-icon-512.png) | 512 × 512 | Opaque RGB PNG, 24-bit | 296,719 |
| [Developer header](exports/developer-header-4096x2304.jpg) | 4096 × 2304 | Opaque RGB JPEG | 930,287 |

Both exports are below 1 MB. Metadata and SHA-256 hashes are recorded in [asset-manifest.json](asset-manifest.json). The live Console requires JPG or 24-bit PNG without transparency, so these exports follow that control; the [Google help article](https://support.google.com/googleplay/android-developer/answer/9873827?hl=en) still describes a 32-bit PNG icon. Website and featured app are optional; promotional text is limited to 140 characters.

The art was generated with the built-in image-generation tool. Original PNGs remain in `source/`; these are generated artwork, not game screenshots. Root maintains the generation prompts in `prompts.txt`. This documentation pass read metadata only and did not modify images.

## Promotional text

**English — 118 characters**

Clever puzzles, charming pets, fresh ideas. We craft mobile games and explore new technology to shape what comes next.

**Vietnamese — Console counter 133/140; local Unicode length 133**

Câu đố thú vị, thú cưng đáng yêu, ý tưởng mới. Chúng tôi sáng tạo game di động và khám phá công nghệ cho những trải nghiệm tương lai.

Copy describes the creative direction without claiming a team size, awards, install counts or AI product capabilities.

## Page settings

- Featured app: **Critter Jam: Color Sort Puzzle**, package `com.ntklstudio.critterjam`, because it is the only published app currently verified on the [public developer collection](https://play.google.com/store/apps/developer?id=YoloStudioGlobal). Alien Pet has not been published.
- Website: **[cj.suri.house](https://cj.suri.house/)**, explicitly selected by the owner and saved with the developer page.
- Save proof: `verification/console-saved.png` in the full source folder. It confirms Console persistence, not immediate public visibility. The ZIP intentionally excludes proof screenshots and source masters.

## Handoff ZIP

`YoloStudioGlobal-DeveloperPage.zip` contains the two upload exports, this README, `asset-manifest.json` and `prompts.txt`. Large source masters and Console proof screenshots stay outside the ZIP.
