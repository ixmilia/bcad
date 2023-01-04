﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IxMilia.BCad;
using IxMilia.BCad.CommandLine;
using IxMilia.BCad.Display;
using IxMilia.BCad.Rpc;
using Nerdbank.Streams;
using PhotinoNET;
using StreamJsonRpc;

namespace bcad
{
    class Program
    {
        private const string WindowTitle = "BCad";

        static async Task PerformAutomatedStepsAsync(IWorkspace workspace, CadArguments arguments)
        {
            if (arguments.DrawingFile is not null)
            {
                await workspace.ExecuteCommand("File.Open", arguments.DrawingFile.FullName);
            }

            if (arguments.BatchFile is not null)
            {
                var batchCommandScript = File.ReadAllText(arguments.BatchFile.FullName);
                var result = await workspace.ExecuteTokensFromScriptAsync(batchCommandScript);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            var arguments = CommandLineParser.Parse(args);

            if (!arguments.ShowUI)
            {
                FileDialogs.GetConsole();
                Console.WriteLine("console mode");
                var consoleWorkspace = new ConsoleWorkspace();
                PerformAutomatedStepsAsync(consoleWorkspace, arguments).Wait();
                Environment.Exit(0);
            }

            FileDialogs.Init();

            var serverStream = new SimplexStream();
            var clientStream = new SimplexStream();
            var encoding = new UTF8Encoding(false);
            var writer = new StreamWriter(serverStream, encoding);
            var formatter = new JsonMessageFormatter(encoding);
            Serializer.PrepareSerializer(formatter.JsonSerializer);
            var messageHandler = new NewLineDelimitedMessageHandler(clientStream, serverStream, formatter);
            var server = new FullServer(messageHandler);

            server.Workspace.SettingsService.RegisterSetting(DisplaySettingsNames.Theme, typeof(string), "xp/98.css");

            Action CloseApplication = () => { };

            server.Agent.IsReady += (o, e) =>
            {
                var _ = Task.Run(async () =>
                {
                    await PerformAutomatedStepsAsync(server.Workspace, arguments);
                    if (arguments.BatchFile is not null)
                    {
                        CloseApplication();
                    }
                });
            };

            // try to load settings
            string[] settingsLines = Array.Empty<string>();
            var settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bcadconfig");
            try
            {
                settingsLines = File.ReadAllLines(settingsFilePath);
                server.Workspace.SettingsService.LoadFromLines(settingsLines);
                server.Workspace.Update(isDirty: false);
            }
            catch
            {
                // don't really care if it failed
            }

#if DEBUG
            var allowDebugging = true;
            var logVerbosity = 1;
            var clientArguments = new[] { "--", "debug" };
#else
            var allowDebugging = false;
            var logVerbosity = 0;
            var clientArguments = new[] { "--" };
#endif

            var window = new PhotinoWindow()
                .SetUseOsDefaultSize(true)
                .SetContextMenuEnabled(false)
                .Center()
                .SetDevToolsEnabled(allowDebugging)
                .SetResizable(true)
                .SetLogVerbosity(logVerbosity)
                .RegisterWebMessageReceivedHandler((object sender, string message) =>
                {
                    try
                    {
                        // forward JSON from client to server
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                    catch
                    {
                        // don't let this die
                    }
                });

            window.RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
            {
                contentType = "text/javascript";
                var script = $"var clientArguments = [{string.Join(", ", clientArguments.Select(arg => $"\"{arg}\""))}];";
                return new MemoryStream(Encoding.UTF8.GetBytes(script));
            });

            var fileSystemService = new FileSystemService(action =>
            {
                window.Invoke(action);
            });
            server.Workspace.RegisterService(fileSystemService);

            server.Workspace.WorkspaceChanged += (sender, args) => SetTitle(window, server.Workspace);

            var doHardClose = false;
            window.WindowClosing += (sender, args) =>
            {
                if (doHardClose)
                {
                    // close immediately
                    return false;
                }

                if (server.Workspace.IsDirty)
                {
                    var _ = Task.Run(async () =>
                    {
                        var result = await server.Workspace.PromptForUnsavedChanges();
                        if (result != UnsavedChangesResult.Cancel)
                        {
                            doHardClose = true;
                            window.Close();
                        }
                    });

                    // don't close yet
                    return true;
                }

                return false;
            };

            CloseApplication = () =>
            {
                doHardClose = true;
                window.Close();
            };

            var _ = Task.Run(async () =>
            {
                var reader = new StreamReader(clientStream, encoding);
                while (true)
                {
                    try
                    {
                        var line = await reader.ReadLineAsync();
                        window.SendWebMessage(line);
                    }
                    catch
                    {
                        // don't let this die
                    }
                }
            });

            server.Start();
            var indexPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
            window.Load(indexPath);
            window.WaitForClose();

            // try to save settings
            try
            {
                var newSettingsContents = server.Workspace.SettingsService.WriteWithLines(settingsLines);
                File.WriteAllText(settingsFilePath, newSettingsContents);
            }
            catch
            {
                // don't really care if it failed
            }
        }

        private static void SetTitle(PhotinoWindow window, IWorkspace workspace)
        {
            var drawingName = workspace.Drawing.Settings.FileName is null
                ? "(Untitled)"
                : Path.GetFileName(workspace.Drawing.Settings.FileName);
            var dirtyMarker = workspace.IsDirty ? " *" : "";
            var title = $"{WindowTitle} [{drawingName}]{dirtyMarker}";
            window.SetTitle(title);
        }
    }
}
