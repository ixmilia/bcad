#!/bin/sh -e

CORE_TEST=./src/IxMilia.BCad.Core.Test/IxMilia.BCad.Core.Test.csproj
FILE_HANDLER_TEST=./src/IxMilia.BCad.FileHandlers.Test/IxMilia.BCad.FileHandlers.Test.csproj
dotnet restore $CORE_TEST
dotnet restore $FILE_HANDLER_TEST
dotnet test $CORE_TEST
dotnet test $FILE_HANDLER_TEST
