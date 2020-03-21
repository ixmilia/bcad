#!/bin/bash -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"

# IxMilia.Dxf needs a custom invocation
"$_SCRIPT_DIR/src/IxMilia.Dxf/build-and-test.sh" --notest

# restore packages
dotnet restore

# build .NET
dotnet build

# test
dotnet test --no-restore --no-build

# build electron
pushd "$_SCRIPT_DIR/src/bcad"
npm i
npm run pack
popd
