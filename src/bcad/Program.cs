using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IxMilia.BCad.Rpc;
using Nerdbank.Streams;
using PhotinoNET;
using StreamJsonRpc;

namespace bcad
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string windowTitle = "BCad";
            var serverStream = new SimplexStream();
            var clientStream = new SimplexStream();
            var encoding = new UTF8Encoding(false);
            var writer = new StreamWriter(serverStream, encoding);
            var formatter = new JsonMessageFormatter(encoding);
            formatter.JsonSerializer.Converters.Add(new KeyEnumConverter());
            var messageHandler = new NewLineDelimitedMessageHandler(clientStream, serverStream, formatter);
            var server = new FullServer(messageHandler);

            server.Agent.IsReady += (o, e) =>
            {
                if (args.Length == 1)
                {
                    var _ = server.Workspace.ExecuteCommand("File.Open", args[0]);
                }
            };

            var allowDebugging = false;
            var window = new PhotinoWindow()
                .SetTitle(windowTitle)
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
                    // forward JSON from client to server
                    writer.WriteLine(message);
                    writer.Flush();
                });

            var fileSystemService = new FileSystemService(action =>
            {
                window.Invoke(action);
            });
            server.Workspace.RegisterService(fileSystemService);

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
                    catch (Exception _ex)
                    {
                    }
                }
            });

            server.Start();
            var indexPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
            window.Load(indexPath);
            window.WaitForClose();
        }
    }
}
