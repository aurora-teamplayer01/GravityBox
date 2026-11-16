#!/usr/bin/env python3
"""Read-only source audit; manual/device checks can never pass from this script."""
import json, re, subprocess, sys
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
record('Service boot', 'BLOCKED' if '#if UNITY_ANDROID && !UNITY_EDITOR' in services else 'MANUAL',
       'Inspect iOS lifecycle and all SDK initializers; no automatic compliance assertion.')
record('iOS AdMob app ID', 'BLOCKED' if not ios_id or '3940256099942544' in ios_id else 'MANUAL',
       'Real iOS app and ad units must be tested. IDs deliberately omitted from report.')
configs = list((repo / 'Assets').rglob('GoogleService-Info.plist'))
record('Firebase iOS config', 'FOUND_REVIEW_REQUIRED' if configs else 'BLOCKED',
       'Check bundle match and generated archive; file presence alone is insufficient.')
record('Native privacy manifests', 'FOUND_REVIEW_REQUIRED' if list((repo / 'Assets').rglob('PrivacyInfo.xcprivacy')) else 'MANUAL',
       'Also inspect resolved CocoaPods, application required-reason APIs and archive privacy report.')
packages = json.loads(read('Packages/manifest.json') or '{}').get('dependencies', {})
record('IAP provider', 'MANUAL', 'Unity Purchasing dependency ' + ('present' if 'com.unity.purchasing' in packages else 'not present') + '; verify any alternative StoreKit provider and real product handling.')
record('Development install script', 'NEEDS_FIX' if 'Release-iphoneos/COghe.app' in read('Tools/build-venom-ios.sh') else 'MANUAL',
       'Do not use USB install script as App Store archive workflow.')
for name in ['Apple team and Bundle ID ownership', 'Native age/consent QA', 'TestFlight device QA', 'App Privacy and rating', 'iOS screenshot parity', 'Archive validation and upload']:
    record(name, 'MANUAL_NOT_RUN', 'Needs actual receiving-machine evidence.')
report = dict(source_commit=subprocess.check_output(['git', '-C', str(repo), 'rev-parse', 'HEAD'], text=True).strip(),
              unity=read('ProjectSettings/ProjectVersion.txt').splitlines()[0],
              release_ready=False, scope='Static audit only', checks=checks)
print(json.dumps(report, indent=2))
sys.exit(2 if any(c['status'] == 'BLOCKED' for c in checks) else 0)
