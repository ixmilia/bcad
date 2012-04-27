using System;
using System.Linq;
using System.Threading;
using BCad.Extensions;
using BCad.Objects;
using Xunit;

namespace BCad.Test
{
    public abstract class AbstractDrawingTests : IDisposable
    {
        private ManualResetEvent valueRequested = new ManualResetEvent(false);
        private ManualResetEvent valueReceived = new ManualResetEvent(false);
        private ManualResetEvent commandExecutionStarted = new ManualResetEvent(false);
        private ManualResetEvent commandExecutionComplete = new ManualResetEvent(false);

        public AbstractDrawingTests()
        {
            this.Host = TestHost.CreateHost();

            Workspace.CommandExecuting += new CommandExecutingEventHandler((s, e) => commandExecutionStarted.Set());
            Workspace.CommandExecuted += new CommandExecutedEventHandler((s, e) => commandExecutionComplete.Set());
            InputService.ValueRequested += new ValueRequestedEventHandler((s, e) => valueRequested.Set());
            InputService.ValueReceived += new ValueReceivedEventHandler((s, e) => valueReceived.Set());
        }

        protected void Wait(ManualResetEvent manualResetEvent, int msTimeout = 100)
        {
            if (!manualResetEvent.WaitOne(msTimeout))
            {
                throw new TimeoutException("Event timeout failed");
            }
        }

        protected void Execute(string commandName)
        {
            commandExecutionStarted.Reset();
            commandExecutionComplete.Reset();
            valueRequested.Reset();
            Workspace.ExecuteCommand(commandName);
            Wait(commandExecutionStarted);
        }

        protected void WaitForCompletion()
        {
            Wait(commandExecutionComplete);
        }

        protected void WaitForRequest()
        {
            Wait(valueRequested);
            valueRequested.Reset();
        }

        protected void Push(object obj)
        {
            WaitForRequest();
            valueReceived.Reset();
            InputService.PushValue(obj);
            Wait(valueReceived);
        }

        protected void AwaitingCommand()
        {
            Assert.Equal(InputType.Command, InputService.DesiredInputType);
        }

        protected void AwaitingPoint()
        {
            Assert.Equal(InputType.Point, InputService.DesiredInputType);
        }

        protected void Complete()
        {
            InputService.PushValue(null);
            WaitForCompletion();
        }

        protected void Cancel()
        {
            InputService.Cancel();
            WaitForCompletion();
        }

        protected void VerifyLayerContains(string layerName, Entity obj)
        {
            var x = Workspace.Document.Layers[layerName];
            Assert.True(x.Objects.Any(o => o.EquivalentTo(obj)));
        }

        protected void VerifyLayerDoesNotContain(string layerName, Entity obj)
        {
            var x = Workspace.Document.Layers[layerName];
            Assert.False(x.Objects.Any(o => o.EquivalentTo(obj)));
        }

        public void Dispose()
        {
            if (valueRequested != null)
                valueRequested.Dispose();
            if (valueReceived != null)
                valueReceived.Dispose();
            if (commandExecutionStarted != null)
                commandExecutionStarted.Dispose();
            if (commandExecutionComplete != null)
                commandExecutionComplete.Dispose();
        }

        protected TestHost Host { get; set; }
        protected IWorkspace Workspace { get { return Host.Workspace; } }
        protected IInputService InputService { get { return Host.InputService; } }
    }
}
