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
SOLUTION=$SCRIPT_DIR/src/BCad.sln
CORE_TEST=$SCRIPT_DIR/src/IxMilia.BCad.Core.Test/IxMilia.BCad.Core.Test.csproj
FILE_HANDLER_TEST=$SCRIPT_DIR/src/IxMilia.BCad.FileHandlers.Test/IxMilia.BCad.FileHandlers.Test.csproj
dotnet restore $SOLUTION
# can't build BCad.sln because of WPF dependency, but this project has references to everything necessary
dotnet build $FILE_HANDLER_TEST -c $CONFIGURATION

# test
dotnet test $CORE_TEST -c $CONFIGURATION --no-restore --no-build
dotnet test $FILE_HANDLER_TEST -c $CONFIGURATION --no-restore --no-build
