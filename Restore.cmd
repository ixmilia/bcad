set nugetexe=%~dp0.nuget\NuGet.exe

%nugetexe% restore %~dp0BCad.sln
%Nugetexe% restore %~dp0BCad\packages.config -SolutionDir %~dp0
