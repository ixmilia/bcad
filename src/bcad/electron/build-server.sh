#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

CONFIGURATION=Debug
TFM=netcoreapp3.1
RID=linux-x64

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

SRC=$_SCRIPT_DIR/../../IxMilia.BCad.Server/IxMilia.BCad.Server.csproj

dotnet restore "$SRC"
dotnet build "$SRC" -c $CONFIGURATION
dotnet publish "$SRC" -c $CONFIGURATION -f $TFM -r $RID
