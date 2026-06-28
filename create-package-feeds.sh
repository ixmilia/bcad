#!/bin/bash -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

ARTIFACTS_DIRECTORY="."
FLATPAK_FEED_PATH="."
WIN_FEED_PATH="."

while [ $# -gt 0 ]; do
  case "$1" in
    --artifacts-directory)
      ARTIFACTS_DIRECTORY=$2
      shift
      ;;
    --flatpak-feed-path)
      FLATPAK_FEED_PATH=$2
      shift
      ;;
    --win-feed-path)
      WIN_FEED_PATH=$2
      shift
      ;;
    *)
      echo "Invalid argument: $1"
      exit 1
      ;;
  esac
  shift
done

destination=`mktemp -d`
echo "package feed location: $destination"

#
# flatpak
#

# import the per-architecture bundles into a single flatpak (ostree) repository
# that can be served over HTTP and added as a remote with `flatpak remote-add`
flatpakDir="$destination/flatpak"
flatpakRepoDir="$flatpakDir/repo"
mkdir -p "$flatpakDir"

# initialize the repository; `archive-z2` is the mode used for repositories
# served over plain HTTP (`flatpak build-import-bundle` requires an existing repo)
ostree --repo="$flatpakRepoDir" init --mode=archive-z2

for bundle in $ARTIFACTS_DIRECTORY/flatpak-x64/*.flatpak $ARTIFACTS_DIRECTORY/flatpak-arm64/*.flatpak; do
    flatpak build-import-bundle --no-update-summary "$flatpakRepoDir" "$bundle"
done

# generate the repository metadata (summary/appstream) consumed by clients
flatpak build-update-repo "$flatpakRepoDir"

# create archive of the entire thing
tar -zcf $FLATPAK_FEED_PATH -C "$destination/" flatpak

#
# win
#

# copy in packages
winPackagesDir="$destination/win"
mkdir -p "$winPackagesDir"
cp $ARTIFACTS_DIRECTORY/win-x64/*.zip "$winPackagesDir"
cp $ARTIFACTS_DIRECTORY/win-arm64/*.zip "$winPackagesDir"

# copy install script
cp $_SCRIPT_DIR/install.ps1 "$winPackagesDir"

# create archive of the entire thing
tar -zcf $WIN_FEED_PATH -C "$destination/" win

#
# version
#
# get version prefix from version.txt file
VERSION_PREFIX=$(cat $_SCRIPT_DIR/version.txt)
# VERSION_SUFFIX is normally computed on the host (where git works reliably) and
# passed in via the environment; fall back to computing it here if unset
if [ -z "$VERSION_SUFFIX" ]; then
    pushd "$_SCRIPT_DIR/build"
    VERSION_SUFFIX=$(pwsh ./make-version.ps1 -suffix beta)
    popd
fi
echo "${VERSION_PREFIX}-${VERSION_SUFFIX}" | tee $ARTIFACTS_DIRECTORY/version.txt

# clean up
rm -rf $destination
