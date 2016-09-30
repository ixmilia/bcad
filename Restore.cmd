set nugetexe=%~dp0.nuget\NuGet.exe

%nugetexe% restore %~dp0BCad.sln
%Nugetexe% restore %~dp0BCad\packages.config -SolutionDir %~dp0

dotnet restore BCad\project.json
dotnet restore BCad.Core\project.json
dotnet restore BCad.FileHandlers\project.json
dotnet restore IxMilia.Iges\src\IxMilia.Iges\project.json
dotnet restore IxMilia.Step\src\IxMilia.Step\project.json
