BCad
====

A .NET WPF application for simple CAD-like work.

## Building

0. Clone.
1. Run `Init.cmd` in the root of the project to populate the submodules.
2. `msbuild .\BuildAndTest.proj`.

## Running

Run `src\Binaries\Debug\BCad.exe` directly or use `src\deploy.bat <location>` to copy all relevant files
to `<location>` where `BCad.exe` can then be run.
