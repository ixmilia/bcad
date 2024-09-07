#!/bin/bash -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

ARTIFACTS_DIRECTORY="."
DEB_FEED_PATH="."
WIN_FEED_PATH="."

while [ $# -gt 0 ]; do
  case "$1" in
    --artifacts-directory)
      ARTIFACTS_DIRECTORY=$2
      shift
      ;;
    --deb-feed-path)
      DEB_FEED_PATH=$2
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
# deb
#

# copy in packages
packagesDir="$destination/deb"
poolMainDir="$packagesDir/pool/main"
mkdir -p "$poolMainDir"
cp $ARTIFACTS_DIRECTORY/deb-x64/*.deb "$poolMainDir"
cp $ARTIFACTS_DIRECTORY/deb-arm64/*.deb "$poolMainDir"

# create Packages files
architectures='amd64 arm64'
for architecture in $architectures; do
    distsDir="$packagesDir/dists/stable/main/binary-$architecture"
    mkdir -p "$distsDir"
    pushd "$packagesDir"
    dpkg-scanpackages --arch $architecture "pool/" > "$distsDir/Packages"
    cat "$distsDir/Packages" | gzip -9 > "$distsDir/Packages.gz"
    popd
done

# create Release file
pushd "$packagesDir/dists/stable"
$_SCRIPT_DIR/generate-release.sh > Release
popd

# create archive of the entire thing
tar -zcf $DEB_FEED_PATH -C "$destination/" deb

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
pushd "$_SCRIPT_DIR/build"
# get version prefix from version.txt file
VERSION_PREFIX=$(cat $_SCRIPT_DIR/version.txt)
VERSION_SUFFIX=$(pwsh ./make-version.ps1 -suffix beta)
popd
echo "${VERSION_PREFIX}-${VERSION_SUFFIX}" | tee $ARTIFACTS_DIRECTORY/version.txt

# clean up
rm -rf $destination
