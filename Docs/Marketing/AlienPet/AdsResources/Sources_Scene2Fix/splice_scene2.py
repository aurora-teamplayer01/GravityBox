#!/usr/bin/env python3
"""Replace only G's guidance insert; never publish/overwrite the ads kit.

Run with the bundled Python (Pillow required). A real replacement source is
mandatory except for --prepare-only, which only backs up the original and
exports the existing badge treatment. No placeholder footage is synthesized.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path
import re
import shutil
import subprocess as sp
import sys

from PIL import Image, ImageDraw, ImageFont

RUN = Path(__file__).resolve().parent
REPO = RUN.parents[1]
FFMPEG = '/opt/homebrew/bin/ffmpeg'
FFPROBE = '/opt/homebrew/bin/ffprobe'
NAME = 'AlienPet_G_CINEMATIC_ESCAPE_30s_1080x1920'
ORIGINAL = REPO / 'Docs/Marketing/AlienPet/AdsResources/01_CURRENT_ALIEN_PET' / (NAME + '.mp4')
BACKUP = RUN / 'backup' / (NAME + '_before-scene2-fix.mp4')
OUTPUT = RUN / 'review-output' / (NAME + '_scene2-fixed.mp4')
FPS, FIRST, AFTER, TOTAL = 30, 241, 331, 900
EXPECTED_ORIGINAL_SHA256 = '3d490df5a6688b47b384d1cbee750c9548d78cad24fc83e83dd418173c9db1bf'
FONT = REPO / 'Assets/_Game/Venom/Resources/COgheUI/Manrope.ttf'


def run(args, *, capture=False):
    p = sp.run(args, check=True, capture_output=capture, text=capture)
    return p.stdout if capture else None


def digest(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for b in iter(lambda: f.read(1024 * 1024), b''):
            h.update(b)
    return h.hexdigest()


def probe(path, count=False):
    args = [FFPROBE, '-v', 'error']
    if count:
        args += ['-count_frames']
    args += ['-show_streams', '-show_format', '-of', 'json', str(path)]
    return json.loads(run(args, capture=True))


def stream(info, kind):
    return next(s for s in info['streams'] if s['codec_type'] == kind)


def write_json(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n')


def prepare():
    for folder in ('backup', 'work', 'review-output', 'qa'):
        (RUN / folder).mkdir(exist_ok=True)
    if BACKUP.exists():
        assert digest(BACKUP) == EXPECTED_ORIGINAL_SHA256, 'Backup mismatch; never overwrite it.'
    else:
        assert digest(ORIGINAL) == EXPECTED_ORIGINAL_SHA256, 'Current master differs from audited original.'
        shutil.copy2(ORIGINAL, BACKUP)
        assert digest(BACKUP) == EXPECTED_ORIGINAL_SHA256

    # Same dimensions, font placement, color and alpha as cinematic() in the
    # original rebrand script. Compose onto transparent pixels, not old footage.
    badge = Image.new('RGBA', (1080, 1920))
    ImageDraw.Draw(badge).rounded_rectangle((365, 160, 714, 216), radius=28,
                                           fill=(12, 34, 39, 175))
    font = ImageFont.truetype(str(FONT), 22)
    label = 'AI CINEMATIC'
    bbox = font.getbbox(label)
    glyph = Image.new('RGBA', (math.ceil(font.getlength(label)) + 12, 52))
    ImageDraw.Draw(glyph).text((6, -bbox[1]), label, font=font, fill=(243, 249, 239, 255))
    badge.alpha_composite(glyph, ((1080 - glyph.width) // 2, 177))
    badge_path = RUN / 'work' / 'ai-cinematic-badge.png'
    badge.save(badge_path)
    write_json(RUN / 'preparation.json', {
        'status': 'prepared_only_not_rendered',
        'backup': str(BACKUP), 'backup_sha256': digest(BACKUP),
        'replacement_frames': {'start_inclusive': FIRST, 'end_exclusive': AFTER},
        'replacement_seconds': [FIRST / FPS, AFTER / FPS],
        'output_frames': TOTAL, 'output_duration_seconds': TOTAL / FPS,
        'badge': str(badge_path),
        'publication': 'Manual approval required; this script never updates the ads kit.'
    })
    return badge_path


def audio_packets(path):
    data = json.loads(run([
        FFPROBE, '-v', 'error', '-select_streams', 'a:0', '-show_packets',
        '-show_data_hash', 'sha256', '-show_entries',
        'packet=pts_time,dts_time,duration_time,size,data_hash', '-of', 'json', str(path)
    ], capture=True))['packets']
    fingerprint = hashlib.sha256(json.dumps(data, sort_keys=True).encode()).hexdigest()
    return data, fingerprint


def contacts(candidate):
    frames = [240, 241, 270, 300, 330, 331]
    sheet = Image.new('RGB', (216 * len(frames), 840), '#183238')
    d = ImageDraw.Draw(sheet)
    f = ImageFont.truetype(str(FONT), 13)
    for row, (name, path) in enumerate((('BEFORE', BACKUP), ('AFTER', candidate))):
        for col, n in enumerate(frames):
            still = RUN / 'qa' / f'{name.lower()}-frame-{n:03d}.jpg'
            run([FFMPEG, '-y', '-v', 'error', '-i', str(path), '-vf',
                 f'select=eq(n\\,{n}),scale=216:384:flags=lanczos',
                 '-frames:v', '1', '-q:v', '2', str(still)])
            with Image.open(still) as im:
                sheet.paste(im, (col * 216, row * 420 + 30))
            d.text((col * 216 + 5, row * 420 + 5),
                   f'{name} f{n} / {n/FPS:.3f}s', font=f, fill='#f4fff7')
    out = RUN / 'qa' / 'scene2-before-after-and-cuts.jpg'
    sheet.save(out, quality=92)
    return out


def validate(candidate, source, source_in):
    before = probe(BACKUP, count=True)
    after = probe(candidate, count=True)
    v = stream(after, 'video')
    a = stream(after, 'audio')
    assert (v['width'], v['height'], v['r_frame_rate'], v['codec_name']) == (1080, 1920, '30/1', 'h264')
    assert int(v['nb_read_frames']) == TOTAL
    assert abs(float(after['format']['duration']) - 30) < .001
    assert a['codec_name'] == 'aac' and a['sample_rate'] == '48000' and a['channels'] == 2

    old_audio, old_hash = audio_packets(BACKUP)
    new_audio, new_hash = audio_packets(candidate)
    assert old_audio == new_audio, 'AAC packet content or timestamps changed.'
    run([FFMPEG, '-v', 'error', '-xerror', '-i', str(candidate), '-f', 'null', '-'])

    # Content outside replacement remains on its exact original frame. The video
    # is re-encoded at CRF17; this intentionally tests visual similarity rather
    # than falsely asserting compressed video byte identity.
    keep = f"select='not(between(n,{FIRST},{AFTER-1}))',setpts=N/({FPS}*TB)"
    graph = f'[0:v]{keep}[a];[1:v]{keep}[b];[a][b]ssim'
    check = sp.run([FFMPEG, '-hide_banner', '-i', str(BACKUP), '-i', str(candidate),
                    '-filter_complex', graph, '-an', '-f', 'null', '-'], capture_output=True, text=True, check=True)
    (RUN / 'qa' / 'outside-scene-ssim.log').write_text(check.stderr)
    match = re.findall(r'All:([0-9.]+)', check.stderr)
    assert match, 'SSIM output missing.'
    ssim = float(match[-1])
    assert ssim >= .985, f'Unexpected change outside replaced scene: SSIM {ssim}'

    black = sp.run([FFMPEG, '-hide_banner', '-i', str(candidate), '-vf',
                   'blackdetect=d=0.1:pix_th=0.05', '-an', '-f', 'null', '-'],
                   capture_output=True, text=True, check=True)
    (RUN / 'qa' / 'black-detection.log').write_text(black.stderr)
    assert 'black_start:' not in black.stderr, 'Black gap detected.'
    sheet = contacts(candidate)
    report = {
        'technical_validation': 'PASS', 'visual_story_approval': 'pending_human_agent_review',
        'output': str(candidate), 'output_sha256': digest(candidate),
        'original': str(BACKUP), 'original_sha256': digest(BACKUP),
        'replacement': str(source), 'replacement_sha256': digest(source),
        'source_in_seconds': source_in, 'source_used_seconds': 3,
        'replacement_frames': {'start_inclusive': FIRST, 'end_exclusive': AFTER},
        'output_frames': TOTAL, 'output_duration_seconds': float(after['format']['duration']),
        'original_metadata': before, 'output_metadata': after,
        'aac_packets_identical_including_timestamps': True,
        'aac_packet_count': len(old_audio), 'aac_packet_fingerprint_sha256': old_hash,
        'output_aac_packet_fingerprint_sha256': new_hash,
        'outside_replaced_scene_ssim': ssim, 'black_gaps': 0, 'full_decode': 'PASS',
        'contact_sheet': str(sheet),
        'publication': 'Review output only; canonical videos and packages not changed.'
    }
    write_json(RUN / 'qa' / 'validation.json', report)
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, help='Real downloaded Flow replacement video')
    parser.add_argument('--source-in', type=float, default=0, help='Seconds to trim from the start; uses the next 3 seconds')
    parser.add_argument('--prepare-only', action='store_true')
    parser.add_argument('--replace-review', action='store_true', help='May overwrite this task’s review render only')
    args = parser.parse_args()
    if not args.prepare_only and args.source is None:
        parser.error('--source is required (or use --prepare-only).')
    if args.source_in < 0:
        parser.error('--source-in must be nonnegative.')
    badge = prepare()
    if args.prepare_only:
        print('Prepared backup and badge. No replacement supplied or final generated.')
        return

    source = args.source.expanduser().resolve()
    assert source.is_file(), f'Source missing: {source}'
    assert source not in (BACKUP, ORIGINAL, OUTPUT), 'Replacement must be a distinct Flow clip.'
    source_info = probe(source)
    sv = stream(source_info, 'video')
    assert abs(sv['width'] / sv['height'] - 9 / 16) < .02, 'Expected portrait 9:16 Flow footage; no silent crop/stretch.'
    assert float(source_info['format']['duration']) + .001 >= args.source_in + 3, 'Source shorter than requested 3-second insert.'
    if OUTPUT.exists() and not args.replace_review:
        parser.error('Review output already exists; inspect it or explicitly use --replace-review.')

    graph = (
        f'[0:v]split=2[old_a][old_c];'
        f'[old_a]trim=end_frame={FIRST},setpts=N/({FPS}*TB),setsar=1[a];'
        f'[old_c]trim=start_frame={AFTER}:end_frame={TOTAL},setpts=N/({FPS}*TB),setsar=1[c];'
        f'[1:v]trim=start={args.source_in}:end={args.source_in+3},setpts=PTS-STARTPTS,'
        f'fps={FPS},scale=1080:1920:flags=lanczos,setsar=1,format=yuv420p[new];'
        f'[new][2:v]overlay=0:0:shortest=1,trim=end_frame={AFTER-FIRST},setpts=N/({FPS}*TB)[b];'
        f'[a][b][c]concat=n=3:v=1:a=0,setpts=N/({FPS}*TB),setsar=1[v]'
    )
    cmd = [FFMPEG, '-y', '-hide_banner', '-i', str(BACKUP), '-i', str(source),
           '-loop', '1', '-i', str(badge), '-filter_complex', graph,
           '-map', '[v]', '-map', '0:a:0', '-c:v', 'libx264', '-crf', '17', '-preset', 'medium',
           '-pix_fmt', 'yuv420p', '-r', str(FPS), '-fps_mode', 'cfr', '-frames:v', str(TOTAL),
           '-c:a', 'copy', '-t', '30', '-movflags', '+faststart', str(OUTPUT)]
    write_json(RUN / 'render-command.json', cmd)
    run(cmd)
    result = validate(OUTPUT, source, args.source_in)
    print(json.dumps({k: result[k] for k in ('technical_validation', 'output', 'output_frames',
          'aac_packets_identical_including_timestamps', 'outside_replaced_scene_ssim', 'contact_sheet')}, indent=2))


if __name__ == '__main__':
    main()
