using System;
using System.Diagnostics;
using System.IO;
using IxMilia.BCad.FileHandlers;
using StreamJsonRpc;

namespace IxMilia.BCad.Server
{
    public class FullServer
    {

        public ServerAgent Agent { get; private set; }

        public ServerAgent Start(Stream duplexStream)
        {
            return Start(duplexStream, duplexStream);
        }

        public ServerAgent Start(Stream sendingStream, Stream receivingStream)
        {
            Console.Error.WriteLine("starting run");
            var workspace = new RpcServerWorkspace();
            RegisterWithWorkspace(workspace);

            var serverRpc = new JsonRpc(sendingStream, receivingStream);
            var server = new ServerAgent(workspace, serverRpc);
            //System.Diagnostics.Debugger.Launch();
            serverRpc.AddLocalRpcTarget(server);
            ((HtmlDialogService)workspace.DialogService).Agent = server;
            serverRpc.TraceSource.Listeners.Add(new Listener());
            serverRpc.StartListening();
            Console.Error.WriteLine("server listening");

            Agent = server;

            return server;
        }

        private static void RegisterWithWorkspace(IWorkspace workspace)
        {
            workspace.RegisterService(new HtmlDialogService(workspace));

            workspace.ReaderWriterService.RegisterFileHandler(new AscFileHandler(), true, false, ".asc");
            workspace.ReaderWriterService.RegisterFileHandler(new DxfFileHandler(), true, true, ".dxf");
            workspace.ReaderWriterService.RegisterFileHandler(new IgesFileHandler(), true, true, ".igs", "iges");
            workspace.ReaderWriterService.RegisterFileHandler(new JsonFileHandler(), true, true, ".json");
            workspace.ReaderWriterService.RegisterFileHandler(new StepFileHandler(), true, false, ".stp", ".step");
            workspace.ReaderWriterService.RegisterFileHandler(new StlFileHandler(), true, false, ".stl");
        }
    }

    class Listener : TraceListener
    {
        public override void Write(string message)
        {
            Console.Error.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.Error.WriteLine(message);
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
