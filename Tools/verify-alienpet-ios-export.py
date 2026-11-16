#!/usr/bin/env python3
"""Validate the actual generated iOS project (not a claim of device QA/signing)."""
import json, plistlib, sys
from pathlib import Path
root = Path(sys.argv[1] if len(sys.argv)>1 else 'Builds/SpatialLab/iOS')
def plist(path):
    with path.open('rb') as f: return plistlib.load(f)
checks=[]
def check(name, ok): checks.append({'check':name,'passed':bool(ok)})
info=plist(root/'Info.plist'); config=plist(root/'GoogleService-Info.plist')
check('Firebase bundle',config.get('BUNDLE_ID')=='com.gravityboxlab.venom')
check('Firebase iOS app',config.get('GOOGLE_APP_ID')=='1:446362971270:ios:8d705f7bc5e2ebbddc3507')
check('AdMob iOS app',info.get('GADApplicationIdentifier')=='ca-app-pub-7806638519709442~8888402391')
check('Positive build number',str(info.get('CFBundleVersion','')).isdigit() and int(info['CFBundleVersion'])>0)
for key in ['FirebaseDataCollectionDefaultEnabled','FIREBASE_ANALYTICS_COLLECTION_ENABLED','GOOGLE_ANALYTICS_IDFA_COLLECTION_ENABLED','GOOGLE_ANALYTICS_IDFV_COLLECTION_ENABLED','GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE','GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE','GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA','GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS','GOOGLE_ANALYTICS_REGISTRATION_WITH_AD_NETWORK_ENABLED']:
    check(key,info.get(key) is False)
check('Delay GMA measurement',info.get('GADDelayAppMeasurementInit') is True)
check('No ATT purpose string',not info.get('NSUserTrackingUsageDescription'))
check('SKAdNetwork list',bool(info.get('SKAdNetworkItems')))
check('Workspace created',(root/'Unity-iPhone.xcworkspace').is_dir())
check('Pod lock present',(root/'Podfile.lock').is_file())
check('Unity privacy manifest',(root/'UnityFramework/PrivacyInfo.xcprivacy').is_file())
check('Marketing icon',(root/'Unity-iPhone/Images.xcassets/AppIcon.appiconset/Icon-Store-1024.png').is_file())
project=(root/'Unity-iPhone.xcodeproj/project.pbxproj').read_text()
check('Product bundle ID','com.gravityboxlab.venom' in project and 'com.gravityboxlab.prototype' not in project)
check('Firebase configuration in project','GoogleService-Info.plist' in project)
check('Swift resource copy phase','Alien Pet - Embed Firebase Swift resources' in project)
check('Swift resource script',(root/'copy-alienpet-ios-swift-resources.sh').is_file())
result={'scope':'Generated Xcode project only; not runtime/consent/device or App Store validation','version':info.get('CFBundleShortVersionString'),'build':info.get('CFBundleVersion'),'checks':checks,'passed':all(c['passed'] for c in checks)}
print(json.dumps(result,indent=2))
sys.exit(0 if result['passed'] else 1)
