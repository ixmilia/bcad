Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$SolutionDir = Join-Path $PSScriptRoot -ChildPath "src"
$SolutionFile = Join-Path $SolutionDir "BCad.sln"
$NuGetDir = Join-Path $SolutionDir -ChildPath ".nuget"
$NuGetExe = Join-Path $NuGetDir -ChildPath "NuGet.exe"
$NuGetUrl = "https://dist.nuget.org/win-x86-commandline/v3.5.0/NuGet.exe"

# every dotnet core project is eventually referenced off of this one
$TopLevelProject = "$SolutionDir\IxMilia.BCad.FileHandlers.Test\IxMilia.BCad.FileHandlers.Test.csproj"
$CoreTestProject = "$SolutionDir\IxMilia.BCad.Core.Test\IxMilia.BCad.Core.Test.csproj"

# restore packages
If (-Not (Test-Path $NuGetExe)) {
    New-Item -ItemType Directory -Force -Path $NuGetDir
    Invoke-WebRequest $NuGetUrl -OutFile $NuGetExe
}
& $NuGetExe restore "$SolutionDir\BCad\packages.config" -SolutionDir "$SolutionDir"
& dotnet restore $TopLevelProject

# build
& dotnet build $TopLevelProject
& msbuild $SolutionFile

# test
& dotnet test $CoreTestProject
& dotnet test $TopLevelProject
