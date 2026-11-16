# Portable build runbook

Run all game commands from the repository root, not the handoff directory. Install the Unity version in ProjectVersion.txt plus iOS Build Support and the release-eligible Xcode SDK. Check branch/commit against source-revision.json. Never transfer a Personal Team provisioning profile as a production solution.

## Inspect before export

```sh
python3 /path/to/handoff/tools/preflight.py /path/to/GravityBox
```

This is a static audit. Any manual gate remains pending even if structural checks pass. It does not install SDKs or mutate the project.

## Export current Spatial catalog

```sh
cd /path/to/GravityBox
export DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer
unset COGHE_IOS_TEAM COGHE_IOS_PROFILE COGHE_IOS_DEVICE COGHE_BENCHMARK COGHE_PROFILE
COGHE_STORE=1 bash Tools/build-venom-ios.sh --spatial
```

Current source fails this guard until real iOS AdMob app ID is configured. It also needs the services work described in IOS-GAPS.md. Do not silence the guard or use `COGHE_STORE=0` for distribution. The default build without `--spatial` is the historical Origin catalog.

Use the workspace if present, apply team/marketing version/build, validate icon and Info.plist, then Archive from Xcode Organizer. Retain resolved dependencies and exact source commit. `COgheIOSBuilder.BuildSpatial` is the relevant Unity method; its menu label says 50 levels but the scene list comes from current SpatialScenePaths.

## Optional command-line archive on the signed-in Mac

After fixing all code/config gates and selecting the real team:

```sh
# Set RELEASE_TEAM to the verified paid-program team ID on this Mac.
xcodebuild -workspace Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace   -scheme Unity-iPhone -configuration Release -destination 'generic/platform=iOS'   -archivePath Builds/AlienPet-iOS.xcarchive   DEVELOPMENT_TEAM="$RELEASE_TEAM" CODE_SIGN_STYLE=Automatic   -allowProvisioningUpdates archive
```

If no workspace exists, first confirm whether resolver/Pods failed; only use the project directly if no Pods are required. Resolve stale CODE_SIGN_IDENTITY/profile overrides in Xcode rather than forcing a development certificate into archive export. Organizer Validate/Distribute is preferred; `ExportOptions.template.plist` is a template, not an executable upload authorization. Fill `teamID` and validate export options against the installed Xcode help.

For device-install troubleshooting, find the built `.app` by its `CFBundleIdentifier` and actual path. The old script hard-codes COghe.app, while public display name changed. An archive must contain a signed app with com.gravityboxlab.venom, not com.gravityboxlab.prototype.

## What this handoff does not execute

No archive/sign/upload/publish, no Apple account changes, no live IAP purchase, no service project creation. Receiving Mac supplies those credentials and records the actual result.
