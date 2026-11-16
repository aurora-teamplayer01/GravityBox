#!/usr/bin/env python3
import hashlib, json, struct, sys
from pathlib import Path

root = Path(__file__).resolve().parents[1]
errors = []
def fail(s): errors.append(s)
def png(p):
    b = p.read_bytes()
    if b[:8] != b'\x89PNG\r\n\x1a\n': raise ValueError('Not PNG: '+str(p))
    w, h = struct.unpack('>II', b[16:24])
    # Renderer exports RGB. Reject alpha channel and tRNS chunk.
    alpha = b[25] in (4, 6)
    off = 8
    while off + 12 <= len(b):
        size = struct.unpack('>I', b[off:off+4])[0]
        if b[off+4:off+8] == b'tRNS': alpha = True
        off += 12 + size
    return w, h, alpha
m = json.loads((root/'metadata/en-US/listing.json').read_text())
for field, limit in [('name',30),('subtitle',30),('promotional_text',170),('description',4000)]:
    if len(m[field]) > limit: fail(field+' too long')
if len(m['keywords'].encode('utf-8')) > 100: fail('keywords >100 bytes')
for field, value in m.items():
    if (root/'metadata/en-US'/f'{field}.txt').read_text().strip() != value: fail(field+' txt/json mismatch')
if png(root/'assets/icon/AppIcon-1024.png') != (1024,1024,False): fail('Invalid icon dimensions/alpha')
cfg = json.loads((root/'screenshots.json').read_text())
audit = json.loads((root/'qa/render-audit.json').read_text())
if len(audit) != len(cfg['shots']): fail('Missing rendered screenshots')
for a in audit:
    p = root/a['file']
    if png(p) != (a['width'],a['height'],False): fail('Invalid PNG: '+a['file'])
    if hashlib.sha256(p.read_bytes()).hexdigest() != a['sha256']: fail('Rendered image hash mismatch: '+a['file'])
    if not (root/a['source']).is_file(): fail('Missing source: '+a['source'])
    if '/upload/' in a['file'] and not a['iosVerified']: fail('Unverified source in upload directory')
if '--assets' not in sys.argv:
    manifest = json.loads((root/'MANIFEST.json').read_text())
    for item in manifest['files']:
        p = root/item['path']
        if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest() != item['sha256']:
            fail('Package hash mismatch: '+item['path'])
print(json.dumps({'asset_checks_passed':not errors,'screenshots':len(audit),'ios_final_captures':sum(bool(a['iosVerified']) for a in audit),'release_ready':False,'errors':errors},indent=2))
sys.exit(1 if errors else 0)
