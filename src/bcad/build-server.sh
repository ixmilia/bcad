#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

CONFIGURATION=Debug

while [ $# -gt 0 ]; do
  case "$1" in
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

SRC=$_SCRIPT_DIR/../IxMilia.BCad.Server/IxMilia.BCad.Server.csproj
DEST=$_SCRIPT_DIR/bin

dotnet restore "$SRC"
dotnet build "$SRC" -c $CONFIGURATION
dotnet publish "$SRC" -c $CONFIGURATION

if [ -d "$DEST" ]; then
    rm -rf "$DEST"
fi

mkdir -p "$DEST"
cp -r "$_SCRIPT_DIR/../../artifacts/bin/IxMilia.BCad.Server/$CONFIGURATION/netcoreapp3.1/publish" "$DEST"
