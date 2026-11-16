#!/usr/bin/env python3
"""Package only this handoff folder, never the game/signing/build directories."""
import hashlib, json, sys, zipfile
from pathlib import Path

root = Path(__file__).resolve().parents[1]
out = Path(sys.argv[1]).resolve() if len(sys.argv)>1 else root.parent/'AlienPet-AppStore-Handoff-2026-11-16.zip'
if root == out.parent or root in out.parents:
    sys.exit('Write ZIP outside the handoff folder.')
skip = {'node_modules','.git','__pycache__'}
files = sorted(p for p in root.rglob('*') if p.is_file() and not skip.intersection(p.relative_to(root).parts) and p.name not in {'MANIFEST.json','.DS_Store'})
for p in files:
    if p.suffix.lower() in {'.p12','.p8','.mobileprovision','.key','.keystore','.jks','.ipa','.aab','.apk'}:
        sys.exit('Forbidden credential/binary in handoff: '+p.name)
    if p.is_symlink(): sys.exit('Symlink not allowed: '+str(p))
manifest={'package':'AlienPet-AppStore-Handoff-2026-11-16','release_ready':False,'files':[{'path':str(p.relative_to(root)),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]}
mp=root/'MANIFEST.json';mp.write_text(json.dumps(manifest,indent=2)+'\n');files.append(mp)
out.parent.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for p in files:z.write(p,Path('AlienPet-AppStore-Handoff-2026-11-16')/p.relative_to(root))
with zipfile.ZipFile(out) as z:
    bad=z.testzip()
    if bad:sys.exit('Corrupt ZIP entry: '+bad)
digest=hashlib.sha256(out.read_bytes()).hexdigest()
out.with_suffix('.zip.sha256').write_text(digest+'  '+out.name+'\n')
print(json.dumps({'zip':str(out),'bytes':out.stat().st_size,'files':len(files),'sha256':digest},indent=2))
