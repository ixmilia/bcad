#!/bin/sh -e

CORE_TEST=./src/IxMilia.BCad.Core.Test/IxMilia.BCad.Core.Test.csproj
FILE_HANDLER_TEST=./src/IxMilia.BCad.FileHandlers.Test/IxMilia.BCad.FileHandlers.Test.csproj

# IxMilia.Dxf needs a custom invocation
./src/IxMilia.Dxf/build-and-test.sh

# only need to restore/build this project since everything else cascades off of it
dotnet restore $FILE_HANDLER_TEST
dotnet build $FILE_HANDLER_TEST

dotnet test $CORE_TEST
dotnet test $FILE_HANDLER_TEST
