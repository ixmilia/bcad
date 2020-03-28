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

# build electron
pushd "$_SCRIPT_DIR/src/bcad"
npm i
npm run pack
popd

# create deployment file
mkdir -p "$_SCRIPT_DIR/artifacts/publish"
SUFFIX=
if [ "${CONFIGURATION,,}" = "debug" ]; then
  SUFFIX=-debug
fi
FILENAME=bcad-linux-x64$SUFFIX.tar.gz
tar -C "$_SCRIPT_DIR/artifacts/pack" -zcf "$_SCRIPT_DIR/artifacts/publish/$FILENAME" "bcad-linux-x64"

# report final artifact name for GitHub Actions
echo "::set-env name=artifact_file_name::$FILENAME"
