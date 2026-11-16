#!/bin/bash
# Export the current campaign for App Store signing on the receiving Mac.
# Usage: bash Tools/build-alienpet-ios-store.sh 1.0.0 1
set -euo pipefail
if [[ $# != 2 ]]; then echo 'Usage: bash Tools/build-alienpet-ios-store.sh <version> <unused-iOS-build-number>' >&2; exit 2; fi
COGHE_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$COGHE_ROOT"
export COGHE_STORE=1 COGHE_IOS_VERSION="$1" COGHE_IOS_BUILD="$2"
unset COGHE_BENCHMARK COGHE_PROFILE COGHE_IOS_SIMULATOR COGHE_IOS_TEAM COGHE_IOS_PROFILE COGHE_IOS_DEVICE
bash Tools/build-venom-ios.sh --spatial
python3 Tools/verify-alienpet-ios-export.py Builds/SpatialLab/iOS
printf '%s\n' 'Open Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace. Select your distribution team, Archive, Validate, then TestFlight.'
