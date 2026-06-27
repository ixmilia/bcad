#!/bin/sh
# Runs on every dev container start (invoked by devcontainer.json's
# postStartCommand). Idempotent.
#
# 1. Start the system D-Bus bus that the sandboxed org.flatpak.Builder talks to.
# 2. Register the static aarch64 qemu interpreter as a binfmt_misc handler so the
#    GNOME aarch64 SDK's build commands can run while cross-building the aarch64
#    flatpak on this x86_64 host (the native x64 build needs no qemu, so this is
#    harmless there).
#
# binfmt_misc is per-boot kernel state that is empty when the container starts,
# so the handler has to be (re)registered here. The static interpreter is copied
# into the image by the Dockerfile (Ubuntu 26.04 no longer ships
# qemu-user-static, and the dynamically linked qemu-user can't run inside the
# flatpak/bubblewrap sandbox because its shared libraries become unreachable
# once bwrap swaps in the aarch64 runtime). Registering with the "F" (fix-binary)
# flag makes the kernel preload the interpreter, so it keeps working regardless
# of the sandbox's mount namespace.

# system D-Bus bus
mkdir -p /run/dbus
[ -S /run/dbus/system_bus_socket ] || dbus-daemon --system --fork

# binfmt_misc must be mounted before any handler can be registered
mount -t binfmt_misc binfmt_misc /proc/sys/fs/binfmt_misc 2>/dev/null || true

# install the handler spec once (it persists in /var/lib/binfmts). magic/mask are
# the standard aarch64 ELF header match used by qemu's own registration; the
# e_type byte is masked with 0xfe so both ET_EXEC and ET_DYN (PIE) binaries match.
if ! update-binfmts --display qemu-aarch64 >/dev/null 2>&1; then
    update-binfmts --install qemu-aarch64 /usr/bin/qemu-aarch64-static \
        --magic '\x7f\x45\x4c\x46\x02\x01\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x02\x00\xb7\x00' \
        --mask '\xff\xff\xff\xff\xff\xff\xff\x00\xff\xff\xff\xff\xff\xff\xff\xff\xfe\xff\xff\xff' \
        --offset 0 \
        --credentials yes \
        --fix-binary yes
fi

# the spec may be cached as "enabled" in /var/lib/binfmts while the kernel
# registration is empty (fresh binfmt_misc on container start), in which case a
# plain "--enable" is a no-op; disabling first forces a real (re)registration.
update-binfmts --disable qemu-aarch64 2>/dev/null || true
update-binfmts --enable qemu-aarch64 2>/dev/null || true
