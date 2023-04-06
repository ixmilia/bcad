#!/bin/bash -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

DEB_DIR="."
FEED_PATH="."

while [ $# -gt 0 ]; do
  case "$1" in
    --feed-path)
      FEED_PATH=$2
      shift
      ;;
    --deb-dir)
      DEB_DIR=$2
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

# copy in packages
packagesDir="$destination/deb"
poolMainDir="$packagesDir/pool/main"
mkdir -p "$poolMainDir"
cp -r $DEB_DIR/*.deb "$poolMainDir"

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
tar -zcf $FEED_PATH -C "$destination/" deb

# clean up
rm -rf $destination
