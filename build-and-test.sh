#!/bin/bash -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

CONFIGURATION=Debug
RUNTESTS=true

while [ $# -gt 0 ]; do
  case "$1" in
    --configuration|-c)
      CONFIGURATION=$2
      shift
      ;;
    --notest)
      RUNTESTS=false
      ;;
    *)
      echo "Invalid argument: $1"
      exit 1
      ;;
  esac
  shift
done

# IxMilia.Dxf needs a custom invocation
"$_SCRIPT_DIR/src/IxMilia.Dxf/build-and-test.sh" --notest --configuration $CONFIGURATION

# restore packages
dotnet restore

# build .NET
dotnet build -c $CONFIGURATION

# test
if [ "$RUNTESTS" = "true" ]; then
  dotnet test --no-restore --no-build -c $CONFIGURATION
fi
