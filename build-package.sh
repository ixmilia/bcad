#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

ARCHITECTURE=x64
CONFIGURATION=Debug

while [ $# -gt 0 ]; do
  case "$1" in
    --architecture|-a)
      ARCHITECTURE=$2
      shift
      ;;
    --configuration|-c)
      CONFIGURATION=$2
      shift
      ;;
    *)
      echo "Invalid argument: $1"
      exit 1
      ;;
  esac
  shift
done

dpkgdir="$_SCRIPT_DIR/src/dpkg"
destination=`mktemp -d`
echo "temporary package location: $destination"

# copy the files
cp -r $dpkgdir/* $destination

# create the directory structure
mkdir -p $destination/usr/share/bcad

# copy the files
cp -r $_SCRIPT_DIR/artifacts/publish/$CONFIGURATION/bcad-linux-$ARCHITECTURE/* $destination/usr/share/bcad

# map the architecture
LINUX_ARCHITECTURE=$ARCHITECTURE
case "$ARCHITECTURE" in
    x64)
        LINUX_ARCHITECTURE=amd64
        ;;
    # arm64 is already correct
esac

# patch the control file
size=$(du -s $destination/usr/share/bcad | cut -f1)
version=$(cat $_SCRIPT_DIR/version.txt)
sed -i "s/%ARCHITECTURE%/$LINUX_ARCHITECTURE/" $destination/DEBIAN/control
sed -i "s/%SIZE%/$size/" $destination/DEBIAN/control
sed -i "s/%VERSION%/$version/" $destination/DEBIAN/control

# now build the package
dpkg-deb --build $destination $_SCRIPT_DIR/artifacts/packages

# clean up
rm -rf $destination
