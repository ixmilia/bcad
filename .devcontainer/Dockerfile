# tags listed here: https://mcr.microsoft.com/v2/dotnet-buildtools/prereqs/tags/list
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-26.04-amd64

# pull the statically linked aarch64 qemu interpreter from the multiarch image.
# Ubuntu 26.04 no longer ships qemu-user-static, and the dynamically linked
# qemu-user can't run inside the flatpak/bubblewrap sandbox (its libraries
# become unreachable once bwrap swaps in the aarch64 runtime). The static
# binary, registered with the "F" (fix-binary) flag below, is preloaded by the
# kernel and works regardless of the sandbox's mount namespace.
COPY --from=multiarch/qemu-user-static:x86_64-aarch64 /usr/bin/qemu-aarch64-static /usr/bin/qemu-aarch64-static

# This is the unified BCad dev container. The host is x86_64, so linux-x64 is
# built natively while linux-arm64 is produced via NativeAOT cross-compilation.
# A single toolchain serves both targets: clang drives every native link and the
# multi-target llvm-objcopy handles the post-link debuginfo split for either
# architecture (see CppCompilerAndLinker/ObjCopyName in devcontainer.json).

# enable the arm64 (aarch64) architecture via Ubuntu multiarch so the NativeAOT
# cross-link can pull the arm64 C runtime/crt objects from the standard
# /usr/lib/aarch64-linux-gnu sysroot location. arm64 binaries are not in the
# amd64 archive.ubuntu.com mirror, so the existing sources are constrained to
# amd64 and a separate ports.ubuntu.com source is added for arm64. ("resolute"
# is the 26.04 suite name.) The native amd64 C runtime/crt objects are already
# present in the base image, so the native x64 link needs no extra sysroot.
RUN set -eux; \
    sed -i 's|^Components: \(.*\)$|Components: \1\nArchitectures: amd64|' /etc/apt/sources.list.d/ubuntu.sources; \
    printf 'Types: deb\nURIs: http://ports.ubuntu.com/ubuntu-ports/\nSuites: resolute resolute-updates resolute-backports resolute-security\nComponents: main universe restricted multiverse\nArchitectures: arm64\nSigned-By: /usr/share/keyrings/ubuntu-archive-keyring.gpg\n' > /etc/apt/sources.list.d/ubuntu-ports-arm64.sources; \
    dpkg --add-architecture arm64

# install the tooling used to build/package both the native x64 image and the
# cross-compiled aarch64 image (including their Flatpaks) on this x86_64 host.
# The .NET SDKs (10.0 + 8.0) are provided by the dotnet
# devcontainer feature (see devcontainer.json) and installed under
# /usr/share/dotnet, so they are intentionally NOT installed via apt here to
# avoid a second copy in /usr/lib/dotnet shadowing the feature on PATH. Ubuntu
# 26.04 ships a modern flatpak (1.16.x) directly in the archive, so no extra
# PPAs or package feeds are required.
RUN apt-get update \
    && export DEBIAN_FRONTEND=noninteractive \
    && apt-get install -y --no-install-recommends \
    # clang is the C++ compiler/linker driver that NativeAOT
    # (Microsoft.DotNet.ILCompiler) invokes when "dotnet publish" links the
    # native image, for both -r linux-x64 (native) and -r linux-arm64 (cross).
    # CppCompilerAndLinker (see devcontainer.json) must name a binary that
    # actually exists on PATH, hence pinning clang-18.
    clang-18 \
    # llvm-18 provides the multi-target llvm-objcopy-18 used for the post-link
    # debuginfo split. Being architecture-agnostic, it works for both the x64 and
    # arm64 images from a single global ObjCopyName (see devcontainer.json),
    # avoiding the need for per-RID objcopy selection.
    llvm-18 \
    # aarch64 cross binutils provide the arm64-capable ld that clang drives via
    # -fuse-ld=bfd --target=aarch64-linux-gnu when linking the arm64 image.
    binutils-aarch64-linux-gnu \
    # aarch64 cross gcc supplies the arm64 crtbegin/crtend objects that clang
    # pulls in when linking the PIE native image.
    gcc-aarch64-linux-gnu \
    # arm64 libc + crt startup objects and zlib that the native image links
    # against, installed under /usr/lib/aarch64-linux-gnu via multiarch.
    libc6-dev:arm64 \
    zlib1g-dev:arm64 \
    # flatpak tooling (1.16.x in 26.04 is new enough for the current Flathub
    # summary/index format)
    flatpak \
    # dbus-daemon (system bus) and dbus-run-session (session bus) are needed by
    # the sandboxed org.flatpak.Builder, which shells back to the host flatpak
    # via the org.freedesktop.Flatpak portal over D-Bus
    dbus \
    # polkitd provides the org.freedesktop.PolicyKit1 D-Bus service that
    # flatpak build-bundle looks for (24.04+ renamed policykit-1 to polkitd)
    polkitd \
    # fuse3 (fusermount3) is used by flatpak for repo/runtime operations
    fuse3 \
    # eu-strip/eu-elfcompress (debuginfo handling) used during the flatpak build
    elfutils \
    # gdk-pixbuf SVG loader so the .svg app icon can be validated
    librsvg2-common \
    # dpkg-dev (dpkg-deb/dpkg-architecture) for the .deb packaging scripts
    dpkg-dev \
    # binfmt-support provides update-binfmts, used by post-start.sh to register
    # the static aarch64 qemu interpreter copied in above (see that COPY)
    binfmt-support \
    # misc utilities used by the build/packaging scripts
    curl \
    wget \
    tar \
    gzip \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# add the flathub remote so the GNOME runtime/sdk referenced by the flatpak
# manifest can be installed
RUN flatpak remote-add --system --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo

# install the GNOME runtime/sdk referenced by the flatpak manifest for both
# target architectures: x86_64 for the native x64 flatpak and aarch64 for the
# cross-compiled arm64 flatpak.
RUN flatpak install --system -y flathub \
        org.gnome.Platform/x86_64/50 org.gnome.Sdk/x86_64/50 \
        org.gnome.Platform/aarch64/50 org.gnome.Sdk/aarch64/50

# install a current flatpak-builder from flathub; org.flatpak.Builder itself runs
# as the host (x86_64) architecture and drives the per-target build.
RUN flatpak install --system -y flathub org.flatpak.Builder
        