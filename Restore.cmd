set nugetexe=%~dp0src\.nuget\NuGet.exe

%nugetexe% restore %~dp0src\BCad.sln
%Nugetexe% restore %~dp0src\BCad\packages.config -SolutionDir %~dp0src\

dotnet restore src\BCad\project.json
dotnet restore src\BCad.Core\project.json
dotnet restore src\BCad.FileHandlers\project.json
dotnet restore src\IxMilia.Config\src\IxMilia.Config\project.json
dotnet restore src\IxMilia.Iges\src\IxMilia.Iges\project.json
dotnet restore src\IxMilia.Step\src\IxMilia.Step\project.json
