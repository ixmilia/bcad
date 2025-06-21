# tags listed here: https://mcr.microsoft.com/v2/dotnet-buildtools/prereqs/tags/list
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-20.04-cross-arm64

# the base image needs updated keys
RUN wget -O - https://apt.kitware.com/keys/kitware-archive-latest.asc 2>/dev/null \
    | gpg --dearmor - \
    | tee /usr/share/keyrings/kitware-archive-keyring.gpg \
    >/dev/null
RUN echo "deb [signed-by=/usr/share/keyrings/kitware-archive-keyring.gpg] https://apt.kitware.com/ubuntu/ focal main" \
    >/etc/apt/sources.list.d/kitware.list
