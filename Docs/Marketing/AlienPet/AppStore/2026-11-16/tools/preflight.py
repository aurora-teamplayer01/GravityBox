#!/usr/bin/env python3
"""Read-only source audit; manual/device checks can never pass from this script."""
import json, plistlib, re, subprocess, sys
from pathlib import Path

repo = Path(sys.argv[1] if len(sys.argv) > 1 else '.').resolve()
if not (repo / 'ProjectSettings/ProjectVersion.txt').is_file():
    sys.exit('Pass the GravityBox Unity repository root.')
def read(p):
    f = repo / p
    return f.read_text(errors='replace') if f.exists() else ''
services = read('Assets/_Game/Venom/Services/COgheGoogleServices.cs')
ads = read('Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset')
ios_id = re.search(r'^[ \t]*adMobIOSAppId:[ \t]*([^\r\n]*)', ads, re.M)
ios_id = ios_id.group(1).strip() if ios_id else ''
checks = []
def record(name, status, detail): checks.append(dict(check=name, status=status, detail=detail))
record('iOS service boot', 'PASS_STATIC' if '#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR' in services else 'BLOCKED',
       'Native mobile boot present; on-device consent behavior still requires QA.')
record('iOS AdMob app ID', 'PASS_STATIC' if ios_id == 'ca-app-pub-7806638519709442~8888402391' else 'BLOCKED',
       'Matches registered Alien Pet iOS app.')
unit_source=read('Assets/_Game/Venom/Services/COgheAdMob.cs')
ios_units=unit_source.split('#if UNITY_IOS',1)[-1].split('#else',1)[0]
record('iOS ad units', 'PASS_STATIC' if all(x in ios_units for x in ['7806638519709442/1433739156','7806638519709442/1349978676','7806638519709442/5181412470','3940256099942544/1712485313']) else 'BLOCKED',
       'Production units and Google iOS rewarded test unit are platform-scoped; not proof of fill.')
configs=list((repo/'Assets').rglob('GoogleService-Info.plist'))
valid_config=False
try:
    config=plistlib.loads(configs[0].read_bytes()) if len(configs)==1 else {}
    valid_config=(config.get('BUNDLE_ID')=='com.gravityboxlab.venom' and config.get('PROJECT_ID')=='coghe-57f21' and config.get('GOOGLE_APP_ID')=='1:446362971270:ios:8d705f7bc5e2ebbddc3507')
except (ValueError, OSError): pass
record('Firebase iOS config', 'PASS_STATIC' if valid_config else 'BLOCKED',
       'Exactly one matching config required. Client API key intentionally omitted from report.')
record('Native privacy manifests', 'MANUAL',
       'Generated unsigned app includes SDK/Unity manifests in recorded build; validate final signed archive privacy report.')
packages = json.loads(read('Packages/manifest.json') or '{}').get('dependencies', {})
record('IAP provider', 'MANUAL', 'Unity Purchasing dependency ' + ('present' if 'com.unity.purchasing' in packages else 'not present') + '; verify any alternative StoreKit provider and real product handling.')
record('Development install script', 'BLOCKED' if 'Release-iphoneos/COghe.app' in read('Tools/build-venom-ios.sh') else 'PASS_STATIC',
       'Do not use USB install script as App Store archive workflow.')
for name in ['Apple team and Bundle ID ownership', 'Native age/consent QA', 'TestFlight device QA', 'App Privacy and rating', 'iOS screenshot parity', 'Archive validation and upload']:
    record(name, 'MANUAL_NOT_RUN', 'Needs actual receiving-machine evidence.')
report = dict(source_commit=subprocess.check_output(['git', '-C', str(repo), 'rev-parse', 'HEAD'], text=True).strip(),
              unity=read('ProjectSettings/ProjectVersion.txt').splitlines()[0],
              release_ready=False, scope='Static audit of working tree; source_commit is its Git base when uncommitted. Not device proof.', checks=checks)
print(json.dumps(report, indent=2))
sys.exit(2 if any(c['status'] == 'BLOCKED' for c in checks) else 0)
