#!/bin/sh
# UnityFramework links Firebase Swift packages, but Xcode does not embed their
# resource bundles in the containing app automatically. Run on the app target,
# after UnityFramework builds and before app signing; preserve vendor manifests.
set -eu
: "${BUILT_PRODUCTS_DIR:?Xcode build products required}"
: "${TARGET_BUILD_DIR:?Xcode target directory required}"
: "${UNLOCALIZED_RESOURCES_FOLDER_PATH:?Xcode app resources path required}"
resource_destination="$TARGET_BUILD_DIR/$UNLOCALIZED_RESOURCES_FOLDER_PATH"
mkdir -p "$resource_destination"
for required_bundle in Firebase_FirebaseCore Firebase_FirebaseInstallations Firebase_FirebaseRemoteConfig; do
  if [ ! -f "$BUILT_PRODUCTS_DIR/$required_bundle.bundle/PrivacyInfo.xcprivacy" ]; then
    echo "error: Missing Firebase Swift resource manifest: $required_bundle.bundle" >&2
    exit 1
  fi
done
# Only built dependency bundles; never sweep SourcePackages or copy framework
# executables/test fixtures. Names are the vendor's SwiftPM resource bundle IDs.
for resource_bundle in \
  "$BUILT_PRODUCTS_DIR"/Firebase_*.bundle \
  "$BUILT_PRODUCTS_DIR"/GoogleUtilities_*.bundle \
  "$BUILT_PRODUCTS_DIR"/Promises_*.bundle \
  "$BUILT_PRODUCTS_DIR"/nanopb_*.bundle; do
  [ -d "$resource_bundle" ] || continue
  /usr/bin/ditto "$resource_bundle" "$resource_destination/$(basename "$resource_bundle")"
done
