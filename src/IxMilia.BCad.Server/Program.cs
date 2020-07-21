using System;
using System.Composition;
using StreamJsonRpc;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace IxMilia.BCad.Server
{
    class Program
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        static void Main(string[] args)
        {
            var outFiles = new List<string>();
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i] == "--out-file" && args.Length > i + 1)
                {
                    outFiles.Add(args[i + 1]);
                }
                else
                {
                    Console.WriteLine("Arguments must be one or more of `--out-file <path/to/file.ts>`");
                    Environment.Exit(1);
                }
            }

            if (outFiles.Count > 0)
            {
                var generator = new ContractGenerator(outFiles);
                generator.Run();
            }
            else
            {
                new Program().RunAsync().GetAwaiter().GetResult();
            }
        }

        private async Task RunAsync()
        {
            Console.Error.WriteLine("starting run");
            CompositionContainer.Container.SatisfyImports(this);

            var serverRpc = new JsonRpc(Console.OpenStandardOutput(), Console.OpenStandardInput());
            var server = new ServerAgent(Workspace, serverRpc);
            //System.Diagnostics.Debugger.Launch();
            serverRpc.AddLocalRpcTarget(server);
            ((HtmlDialogService)Workspace.DialogService).Agent = server;
            serverRpc.TraceSource.Listeners.Add(new Listener());
            serverRpc.StartListening();
            Console.Error.WriteLine("server listening");

            while (server.IsRunning)
            {
                await Task.Delay(50);
            }
        }
    }

    class Listener : TraceListener
    {
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            base.TraceEvent(eventCache, source, eventType, id);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void Fail(string message)
        {
            base.Fail(message);
        }

        public override void Fail(string message, string detailMessage)
        {
            base.Fail(message, detailMessage);
        }
    }
}
