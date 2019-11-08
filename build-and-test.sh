#!/bin/sh -e

SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"
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

# IxMilia.Dxf needs a custom invocation to generate code
$SCRIPT_DIR/src/IxMilia.Dxf/build-and-test.sh --notest --configuration $CONFIGURATION

# build
# all buildable projects in Linux can be reached from this
FILE_HANDLER_TEST=$SCRIPT_DIR/src/IxMilia.BCad.FileHandlers.Test/IxMilia.BCad.FileHandlers.Test.csproj
CORE_TEST=$SCRIPT_DIR/src/IxMilia.BCad.Core.Test/IxMilia.BCad.Core.Test.csproj
dotnet restore $FILE_HANDLER_TEST
dotnet build $FILE_HANDLER_TEST -c $CONFIGURATION

# test
dotnet test $CORE_TEST -c $CONFIGURATION --no-restore --no-build
dotnet test $FILE_HANDLER_TEST -c $CONFIGURATION --no-restore --no-build
