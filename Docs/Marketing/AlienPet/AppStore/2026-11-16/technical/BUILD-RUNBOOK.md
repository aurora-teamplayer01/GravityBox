# Portable iOS build runbook

Clone branch **IOS** of `git@github.com:aurora-teamplayer01/GravityBox.git`. Run commands at the repository root. Install Unity **6000.3.19f1** with iOS Build Support, Xcode, CocoaPods, Python 3 and Git LFS if required by clone. No Blender step is needed. SDK setup is automated by `Tools/setup-google-sdks.py`; do not copy Library or personal signing profiles.

## Validate and export

```sh
python3 Docs/Marketing/AlienPet/AppStore/2026-11-16/tools/preflight.py .
# Use a version/build not already uploaded to this App Store Connect app.
bash Tools/build-alienpet-ios-store.sh 1.0.0 1
```

The wrapper sets COGHE_STORE=1, clears benchmark/profiler/simulator/local signing environment, validates real service IDs, exports SpatialScenePaths (65 main levels + 2 catalog prototypes), then inspects the generated Xcode project. It does not upload. Do not bypass guards or export the historical Origin/prototype catalog. The compiled release uses actual iOS ad units; a regular non-store test export uses Google test units.

Output: `Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace`. **Open the workspace**, since Google ads use CocoaPods and Firebase uses resolved Swift packages. Logs: `Artifacts/COgheIOS/unity-export.log`. Generated projects/builds are local and ignored by Git.

Verified here: Xcode 26.6, iOS SDK 26.5; Firebase Unity 13.17.0 / native 12.19.0; Google Mobile Ads native 13.11.0; UMP 3.1.0. Dependency ranges may resolve newer compatible native packages on the receiving Mac. Record Podfile.lock, Package.resolved and source commit with the archive and rerun QA.

## Distribution on the receiving Mac

1. Select the paid Apple Developer team owning `com.gravityboxlab.venom`, configure signing in Xcode and create the matching App Store Connect app. No team is hard-coded into the store wrapper.
2. Confirm Alien Pet: Escape Puzzle display name, iPhone+iPad support, minimum iOS 15, version/build, approved icon C, Firebase plist, native closed privacy defaults and embedded SDK manifests.
3. Archive using a generic iOS device destination, validate in Organizer, distribute to TestFlight. Complete native QA before submission. A Release configuration alone is not distribution signing.

Optional archive command after configuring the team:

```sh
xcodebuild -workspace Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone -configuration Release -destination 'generic/platform=iOS' \
  -archivePath Builds/AlienPet-iOS.xcarchive \
  DEVELOPMENT_TEAM="$RELEASE_TEAM" CODE_SIGN_STYLE=Automatic \
  -allowProvisioningUpdates archive
```

`ExportOptions.template.plist` must be filled against the installed Xcode version; it is not an upload authorization or a signing credential. Use Organizer Validate/Distribute when unsure.

## Optional QA exports

```sh
# Non-store test ads. Clear COGHE_STORE inherited from your shell first.
unset COGHE_STORE COGHE_BENCHMARK COGHE_PROFILE
bash Tools/build-venom-ios.sh --spatial
# Optional simulator export (not exercised in this handoff):
bash Tools/build-venom-ios.sh --spatial --simulator
```

Local device install can use COGHE_IOS_TEAM and COGHE_IOS_DEVICE with the existing helper; it finds the built app by bundle ID. These development options are intentionally cleared by the store wrapper. Never click production ads for QA. See IOS-SERVICES.md for native DebugView steps and remaining listing/consent/IAP gates.

The Unity exporter also installs an app-target phase that embeds Firebase Swift resource bundles from the native build products. Do not remove it: UnityFramework links the libraries but the initial generated app omitted those bundles. It preserves vendor manifests and fails when the required Firebase manifests are missing. Inspect qa/ios-native-build.json and verify the final archive contains them.
