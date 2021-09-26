using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
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
            var server = new FullServer();
            var serverStream = new SimplexStream();
            var clientStream = new SimplexStream();

            var window = new PhotinoWindow()
                .SetTitle(windowTitle)
                .SetUseOsDefaultSize(true)
                .SetContextMenuEnabled(false)
                .Center()
                .SetDevToolsEnabled(false)
                .SetResizable(true)
#if DEBUG
                .SetLogVerbosity(1)
#else
                .SetLogVerbosity(0)
#endif
                .RegisterWebMessageReceivedHandler((object sender, string message) =>
                {
                    // forward JSON from client to server
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    var headerMessage = $"Content-Length: {messageBytes.Length}\r\n\r\n";
                    var headerBytes = Encoding.ASCII.GetBytes(headerMessage);
                    serverStream.Write(headerBytes);
                    serverStream.Write(messageBytes);
                    serverStream.Flush();
                });

            var fileSystemService = new FileSystemService(action =>
            {
                window.Invoke(action);
            });
            server.Workspace.RegisterService(fileSystemService);

            var _ = Task.Run(async () =>
            {
                var clientHandler = new HeaderDelimitedMessageHandler(clientStream);
                while (true)
                {
                    try
                    {
                        var message = await clientHandler.ReadAsync(CancellationToken.None);
                        var buffer = new ArrayBufferWriter<byte>();
                        clientHandler.Formatter.Serialize(buffer, message);
                        var messageString = Encoding.UTF8.GetString(buffer.WrittenSpan);
                        window.SendWebMessage(messageString);
                    }
                    catch (Exception _ex)
                    {
                    }
                }
            });

            server.Start(FullDuplexStream.Splice(serverStream, clientStream));
            var indexPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "wwwroot", "index.html");
            window.Load(indexPath);
            window.WaitForClose();
        }
    }
}
