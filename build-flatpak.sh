#!/bin/sh -e

# build-flatpak.sh
#
# Creates a Flatpak distributable for BCad from the prebuilt, self-contained
# binaries produced by `publish.ps1` (located under artifacts/publish/...).
#
# Required tooling (Ubuntu):
#   sudo apt-get update
#   sudo apt-get install -y flatpak
#
#   # Add the Flathub remote (provides the builder, runtime, and sdk used below):
#   flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
#
#   # Install the build tool. The host's flatpak-builder (e.g. Ubuntu 22.04's
#   # 1.2.2) is too old for the GNOME 50+ SDK, so use the flathub-distributed
#   # org.flatpak.Builder (a current release) instead:
#   flatpak install -y flathub org.flatpak.Builder
#
#   # Install the runtime + sdk referenced by src/flatpak/com.ixmilia.BCad.yml
#   # (BCad uses a WebKitGTK window via Photino, hence the GNOME runtime):
#   flatpak install -y flathub org.gnome.Platform//50 org.gnome.Sdk//50
#
# The result is a single-file `bcad-$ARCHITECTURE.flatpak` bundle (e.g.
# bcad-x64.flatpak) written to artifacts/packages/$CONFIGURATION which can be
# installed with:
#   flatpak install --user ./bcad-x64.flatpak

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

flatpakdir="$_SCRIPT_DIR/src/flatpak"
publishdir="$_SCRIPT_DIR/artifacts/publish/$CONFIGURATION/bcad-linux-$ARCHITECTURE"

if [ ! -d "$publishdir" ]; then
    echo "Published binaries not found at: $publishdir"
    echo "Run publish.ps1 for the desired configuration/runtime first."
    exit 1
fi

# map the architecture to the flatpak naming
FLATPAK_ARCHITECTURE=$ARCHITECTURE
case "$ARCHITECTURE" in
    x64)
        FLATPAK_ARCHITECTURE=x86_64
        ;;
    arm64)
        FLATPAK_ARCHITECTURE=aarch64
        ;;
esac

# compute the version
versionPrefix=$(cat $_SCRIPT_DIR/version.txt)
versionSuffix="0"
if [ "$VERSION_SUFFIX" != "" ]; then
    versionSuffix="$VERSION_SUFFIX"
fi
version="$versionPrefix.$versionSuffix"
releaseDate=$(date +%Y-%m-%d)

# stage the manifest, metadata, and payload in a temporary build directory
#
# this must live somewhere visible to the sandboxed org.flatpak.Builder AND on
# the container's overlay filesystem. Two constraints apply:
#   * flatpak never shares a set of reserved host directories with the sandbox,
#     even under --filesystem=host: notably /tmp (replaced by a private sandbox
#     tmp), /root, /etc and /var. Running as root, $HOME is /root, so neither
#     /tmp nor /root/.cache is visible to the builder.
#   * the dev container's workspace is a 9p bind mount with poor support for the
#     hardlink-heavy ostree cache flatpak-builder writes, so it can't be used.
# /opt is on the overlay filesystem and is not a reserved path, so it satisfies
# both. Override with BCAD_FLATPAK_STAGING if needed.
stagingbase="${BCAD_FLATPAK_STAGING:-/opt/bcad-flatpak}"
mkdir -p "$stagingbase"
staging=`mktemp -d -p "$stagingbase"`
echo "temporary flatpak staging location: $staging"

cp "$flatpakdir/com.ixmilia.BCad.yml" "$staging/"
cp "$flatpakdir/com.ixmilia.BCad.desktop" "$staging/"
cp "$flatpakdir/com.ixmilia.BCad.metainfo.xml" "$staging/"
cp "$_SCRIPT_DIR/src/javascript-client/src/bcad.svg" "$staging/bcad.svg"

# copy the prebuilt, self-contained binaries
mkdir -p "$staging/bcad-publish"
cp -r "$publishdir/." "$staging/bcad-publish/"

# patch version information into the appstream metadata
sed -i "s/%VERSION%/$version/" "$staging/com.ixmilia.BCad.metainfo.xml"
sed -i "s/%RELEASE_DATE%/$releaseDate/" "$staging/com.ixmilia.BCad.metainfo.xml"

# build the flatpak into a local repository
builddir="$staging/build"
repodir="$staging/repo"
statedir="$staging/.flatpak-builder"

# use the flathub-distributed org.flatpak.Builder (a current release) rather than
# the host's flatpak-builder; distro packages lag behind and break against newer
# runtimes (e.g. Ubuntu 22.04's 1.2.2 uses the legacy "appstream-compose" tool
# that the GNOME 50+ SDK no longer ships, which provides "appstreamcli compose").
if ! flatpak info org.flatpak.Builder >/dev/null 2>&1; then
    echo "org.flatpak.Builder is not installed."
    echo "Install it with:"
    echo "  flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo"
    echo "  flatpak install -y flathub org.flatpak.Builder"
    exit 1
fi

# org.flatpak.Builder is sandboxed and shells back to the host flatpak via the
# org.freedesktop.Flatpak portal, which it reaches over a D-Bus *session* bus;
# dbus-run-session provides one (and auto-activates the portal). The *system* bus
# must also be running -- in the dev container that is started on container start
# (see .devcontainer/devcontainer.json postStartCommand).
#
# Ensure the per-user XDG runtime dir (/run/user/$UID) exists. When the sandboxed
# org.flatpak.Builder runs each build command, it spawns "flatpak build" back on
# the host, and that host process inherits the sandbox's XDG_RUNTIME_DIR of
# /run/user/$UID. flatpak allocates a per-build "instance id" directory under
# there; if the directory is missing (it lives on a tmpfs that is empty on
# container start, and nothing creates it when running headless as root) the
# build fails with "Unable to allocate instance id". /run is reserved by the
# flatpak sandbox, so this can't be redirected elsewhere -- the directory simply
# has to exist on the host.
runtimedir="/run/user/$(id -u)"
mkdir -p "$runtimedir"
chmod 700 "$runtimedir"

# --disable-rofiles-fuse: rofiles-fuse can't mount on the container's overlay/9p
# filesystems (and trips a locale-dir migration bug in flatpak-builder); skipping
# it is the standard approach for containerized builds and is safe here since the
# module just copies prebuilt files into the prefix.
dbus-run-session -- flatpak run org.flatpak.Builder \
    --arch="$FLATPAK_ARCHITECTURE" \
    --state-dir="$statedir" \
    --disable-rofiles-fuse \
    --force-clean \
    --repo="$repodir" \
    "$builddir" \
    "$staging/com.ixmilia.BCad.yml"
# export a single-file bundle
#
# --runtime-repo records where the GNOME runtime can be fetched so that a user
# installing the bundle has the dependency resolved/installed automatically
# (flatpak will offer to add the flathub remote on install) rather than having
# to install org.gnome.Platform//50 manually beforehand.
outputdir="$_SCRIPT_DIR/artifacts/packages/$CONFIGURATION"
mkdir -p "$outputdir"
# include the architecture in the bundle name so the x64 and arm64 builds don't
# collide when published side by side
bundlepath="$outputdir/bcad-$ARCHITECTURE.flatpak"
flatpak build-bundle \
    --arch="$FLATPAK_ARCHITECTURE" \
    --runtime-repo=https://flathub.org/repo/flathub.flatpakrepo \
    "$repodir" \
    "$bundlepath" \
    com.ixmilia.BCad

echo "flatpak bundle created at: $bundlepath"

# clean up
rm -rf "$staging"
