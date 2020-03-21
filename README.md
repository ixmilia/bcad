BCad
====

An Electron-based application for simple CAD-like work.

## Build dependencies

1. [.NET Core SDK 3.1](https://dotnet.microsoft.com/download) configured so that the `dotnet` tool is on the path.
2. [Node.js/NPM](https://nodejs.org) LTS.

## Building

0. Clone.
1. Run `init.cmd`/`init.sh` in the root of the project to populate the submodules.
2. `build-and-test.cmd`/`build-and-test.sh`.

## Debugging

To run locally after building:

``` bash
cd src/bcad
npm start
```

## Running

Final app is placed in `artifacts/pack/bcad-win32-x64`/`artifacts/pack/bcad-linux-x64`.

See [README.md](src/bcad/README.md) in `src/bcad` for details on building/running via WSL that I needed on my local box.
