// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using StreamJsonRpc;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IxMilia.BCad.Server
{
    class Program
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        static void Main(string[] args)
        {
            new Program().RunAsync().GetAwaiter().GetResult(); ;
        }

        private async Task RunAsync()
        {
            Console.Error.WriteLine("starting run");
            CompositionContainer.Container.SatisfyImports(this);

            var serverRpc = new JsonRpc(Console.OpenStandardOutput(), Console.OpenStandardInput());
            var server = new ServerAgent(Workspace, serverRpc);
            //System.Diagnostics.Debugger.Launch();
            serverRpc.AddLocalRpcTarget(server);
            ((FileSystemService)Workspace.FileSystemService).Rpc = serverRpc;
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
