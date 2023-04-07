BCad
====

An Electron-based application for simple CAD-like work.

## Build dependencies

1. [.NET Core SDK 3.1](https://dotnet.microsoft.com/download) configured so that the `dotnet` tool is on the path.
2. [Node.js/NPM](https://nodejs.org) LTS.
3. [PowerShell Core](https://github.com/PowerShell/PowerShell/releases).

## Building

0. Clone.
1. Run `init.ps1` in the root of the project to populate the submodules.
2. `./build-and-test.ps1`

## Debugging

To run locally after building:

``` bash
cd src/bcad/electron
npm start
```

## Running

Final app is placed in `artifacts/pack/bcad-[(linux|darwin|win32)]-x64`.

See [README.md](src/bcad/electron/README.md) in `src/bcad/electron` for details on building/running via WSL that I needed on my local box.

## Ubuntu packages

``` bash
# `[arch=arm64]` is also available
echo "deb [arch=amd64] https://files.ixmilia.com/bcad/deb stable main" | sudo tee /etc/apt/sources.list.d/ixmilia.bcad.list
```

``` bash
sudo apt update --allow-insecure-repositories
sudo apt install bcad
```
