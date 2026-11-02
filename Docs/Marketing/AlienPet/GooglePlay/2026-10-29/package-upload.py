#!/usr/bin/env python3
"""Build a reviewed-image upload pack; excludes APKs, keys and credentials."""
import hashlib
import json
from pathlib import Path
import zipfile

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[4]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def public_source(value):
    value = str(value)
    prefix = str(REPO) + "/"
    return value[len(prefix):] if value.startswith(prefix) else value

def main():
    audit = json.loads((ROOT / "qa/screenshot-render-audit.json").read_text())
    copy = json.loads((ROOT / "listing-copy.en-US.json").read_text())
    counts = {kind: sum(item.get("kind") == kind for item in audit)
              for kind in ("phone", "tablet-7in", "tablet-10in")}
    assert counts["phone"] == 6 and counts["tablet-7in"] >= 4 and counts["tablet-10in"] >= 4, counts
    for item in audit:
        assert item["hasAlpha"] is False
        assert sha(ROOT / item["file"]) == item["sha256"], item["file"]
    text = (f"TITLE ({len(copy['title'])}/30)\n{copy['title']}\n\n"
            f"SHORT DESCRIPTION ({len(copy['short_description'])}/80)\n{copy['short_description']}\n\n"
            f"FULL DESCRIPTION ({len(copy['full_description'])}/4000)\n{copy['full_description']}\n")
    (ROOT / "listing-copy.en-US.txt").write_text(text)
    manifest = {
        "title": copy["title"], "locale": "en-US", "date": "2026-10-29",
        "status": "Prepared final visual/copy assets; upload and publication status are tracked separately in the Console status record.",
        "counts": counts,
        "upload_order": {kind: [item["file"] for item in audit if item.get("kind") == kind] for kind in counts},
        "icon": "exports/app-icon-512.png", "feature_graphic": "exports/feature-graphic.png",
        "images": [{k: item[k] for k in ("id", "file", "width", "height", "bytes", "hasAlpha", "sha256")} for item in audit],
        "copy_character_counts": {key: len(copy[key]) for key in ("title", "short_description", "full_description")},
        "preview": "screenshot-review.html",
        "quality_checks": "Opaque PNG dimensions, source/crop bounds, caption overflow and header/image/footer collisions verified by export script. Contact sheet and representative full-resolution outputs inspected.",
        "excluded": "No signing keys, credentials, APK/AAB files, developer account data or private device logs are included."
    }
    (ROOT / "upload-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    feature = json.loads((ROOT / "feature-art-provenance.json").read_text())
    provenance = {
        "feature_art": feature,
        "screenshots": [{"id": item["id"], "file": item["file"],
                         "source": public_source(item["source"]),
                         "source_sha256": item["sourceSha256"],
                         "crop": item.get("crop"),
                         "notes": item.get("note", item.get("transform"))}
                        for item in audit],
        "tablet_note": "7-inch-equivalent and 10-inch-equivalent Android emulator capture sets are separate native captures. These are not evidence of physical device performance. Source overviews do not certify complete puzzle solves.",
        "screenshot_policy": "Actual game source pixels are only uniformly scaled and caption-framed. No gameplay, creature or UI element is synthesized or retouched. Phone crop rectangles exclude documented outer margins and HUD; tablet frames are preserved whole.",
        "icon_note": "Approved text-free icon C, mechanically resized whole to 512x512."
    }
    (ROOT / "provenance.json").write_text(json.dumps(provenance, indent=2) + "\n")
    readme = """ALIEN PET: ESCAPE PUZZLE — GOOGLE PLAY UPLOAD PACK

Upload exports/app-icon-512.png as the app icon and exports/feature-graphic.png
as the feature graphic. The feature is illustrative key art.

Upload the six numbered exports/phone/ images in order. Upload the four numbered
images in exports/tablet-7in/ and exports/tablet-10in/ to their matching tablet
slots. They come from distinct actual native tablet emulator captures.

English title and descriptions: listing-copy.en-US.txt (also JSON with evidence).
Preview all art: screenshot-review.html. Keep its exports/ and design-assets/
folders together when opening it. A static preview is in qa/.

upload-manifest.json records dimensions, hashes and order. provenance.json
records illustration/source distinctions and image transformations. No signing
keys, build artifacts, credentials or private device logs are included.

Upload, submission and publication status are tracked separately in the Console status record.
"""
    files = [ROOT / item["file"] for item in audit]
    files += [ROOT / name for name in ["listing-copy.en-US.txt", "listing-copy.en-US.json", "upload-manifest.json",
              "provenance.json", "feature-art-provenance.json", "screenshot-review.html",
              "qa/screenshot-contact-sheet.png", "design-assets/Manrope.ttf", "design-assets/ManropeBold.ttf"]]
    target = ROOT / "AlienPet_GooglePlay_UploadPack_2026-10-29.zip"
    with zipfile.ZipFile(target, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as pack:
        pack.writestr("README.txt", readme)
        for file in files:
            pack.write(file, file.relative_to(ROOT).as_posix())
    with zipfile.ZipFile(target) as pack:
        assert pack.testzip() is None
        assert not any(Path(name).suffix.lower() in {".apk", ".aab", ".jks", ".keystore", ".pem", ".p12"} for name in pack.namelist())
    result = {"file": str(target), "bytes": target.stat().st_size, "sha256": sha(target), "files": len(files) + 1, "counts": counts,
              "validation": "ZIP CRCs passed; explicit allowlist excludes credentials and build/signing files."}
    (ROOT / "qa/upload-package-validation.json").write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result, indent=2))

if __name__ == "__main__":
    main()
