#!/bin/sh -e

CORE_TEST=./src/BCad.Core.Test/BCad.Core.Test.csproj
FILE_HANDLER_TEST=./src/BCad.FileHandlers.Test/BCad.FileHandlers.Test.csproj
dotnet restore $CORE_TEST
dotnet restore $FILE_HANDLER_TEST
dotnet test $CORE_TEST
dotnet test $FILE_HANDLER_TEST
