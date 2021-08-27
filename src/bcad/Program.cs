using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IxMilia.BCad.Server;
using Nerdbank.Streams;
using PhotinoNET;
using StreamJsonRpc;

namespace bcad.photino
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string windowTitle = "BCad";
            var server = new FullServer();
            var serverStream = new HalfDuplexStream();
            var clientStream = new HalfDuplexStream();

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
                    try
                    {
                        // intercept the File.Open command (doesn't exist in the core)
                        // {"id":2,"method":"ExecuteCommand","params":{"command":"File.Open"},"jsonrpc":"2.0"}
                        if (message.Contains("File.Open"))
                        {
                            var json = Newtonsoft.Json.Linq.JObject.Parse(message);
                            if (json.Value<string>("method") is string method && method == "ExecuteCommand" &&
                                json["params"].Value<string>("command") is string command && command == "File.Open")
                            {
                                var filePath = OpenFileDialog.OpenFile();
                                if (filePath != null)
                                {
                                    var data = File.ReadAllBytes(filePath);
                                    var base64 = Convert.ToBase64String(data);
                                    var _ = server.Agent.ParseFile(filePath, base64);
                                }

                                return;
                            }
                        }
                    }
                    finally
                    {
                    }

                    // forward JSON from client to server
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    var headerMessage = $"Content-Length: {messageBytes.Length}\r\n\r\n";
                    var headerBytes = Encoding.ASCII.GetBytes(headerMessage);
                    serverStream.Write(headerBytes);
                    serverStream.Write(messageBytes);
                    serverStream.Flush();
                });

            Task.Run(async () =>
            {
                var clientHandler = new HeaderDelimitedMessageHandler(clientStream);
                while (true)
                {
                    var message = await clientHandler.ReadAsync(CancellationToken.None);
                    var buffer = new ArrayBufferWriter<byte>();
                    clientHandler.Formatter.Serialize(buffer, message);
                    var messageString = Encoding.UTF8.GetString(buffer.WrittenSpan);
                    window.SendWebMessage(messageString);
                }
            });

            server.Start(FullDuplexStream.Splice(serverStream, clientStream));
            var indexPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "wwwroot", "index.html");
            window.Load(indexPath);
            window.WaitForClose();
        }
    }
}
