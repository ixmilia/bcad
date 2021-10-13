using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IxMilia.BCad;
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

        [STAThread]
        static void Main(string[] args)
        {
            var serverStream = new SimplexStream();
            var clientStream = new SimplexStream();
            var encoding = new UTF8Encoding(false);
            var writer = new StreamWriter(serverStream, encoding);
            var formatter = new JsonMessageFormatter(encoding);
            formatter.JsonSerializer.Converters.Add(new KeyEnumConverter());
            var messageHandler = new NewLineDelimitedMessageHandler(clientStream, serverStream, formatter);
            var server = new FullServer(messageHandler);

            server.Workspace.SettingsService.RegisterSetting(DisplaySettingsNames.Theme, typeof(string), "xp/98.css");

            server.Agent.IsReady += (o, e) =>
            {
                if (args.Length == 1)
                {
                    var _ = server.Workspace.ExecuteCommand("File.Open", args[0]);
                }
            };

            // try to load settings
            string[] settingsLines = Array.Empty<string>();
            var settingsFilePath = Path.Combine(AppContext.BaseDirectory, ".bcadconfig");
            try
            {
                settingsLines = File.ReadAllLines(settingsFilePath);
                server.Workspace.SettingsService.LoadFromLines(settingsLines);
            }
            catch
            {
                // don't really care if it failed
            }

            var allowDebugging = false;
            var window = new PhotinoWindow()
                .SetUseOsDefaultSize(true)
                .SetContextMenuEnabled(allowDebugging)
                .Center()
                .SetDevToolsEnabled(allowDebugging)
                .SetResizable(true)
#if DEBUG
                .SetLogVerbosity(1)
#else
                .SetLogVerbosity(0)
#endif
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
            SetTitle(window, server.Workspace);

            var fileSystemService = new FileSystemService(action =>
            {
                window.Invoke(action);
            });
            server.Workspace.RegisterService(fileSystemService);

            server.Workspace.WorkspaceChanged += (sender, args) => SetTitle(window, server.Workspace);

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
