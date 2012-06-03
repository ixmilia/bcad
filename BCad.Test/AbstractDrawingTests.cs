using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BCad.Entities;
using BCad.Extensions;
using BCad.Services;
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

        protected void Wait(ManualResetEvent manualResetEvent, int msTimeout = 500)
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
            if (obj == null)
            {
                InputService.PushNone();
            }
            else if (obj is string)
            {
                InputService.PushDirective((string)obj);
            }
            else if (obj is double)
            {
                InputService.PushDistance((double)obj);
            }
            else if (obj is Point)
            {
                InputService.PushPoint((Point)obj);
            }
            else if (obj is SelectedEntity)
            {
                InputService.PushEntity((SelectedEntity)obj);
            }
            else if (obj is IEnumerable<Entity>)
            {
                InputService.PushEntities((IEnumerable<Entity>)obj);
            }
            else
            {
                throw new Exception("Unsupported push type: " + obj.GetType().Name);
            }

            Wait(valueReceived);
        }

        protected void AwaitingCommand()
        {
            Assert.Equal(InputType.Command, InputService.AllowedInputTypes);
        }

        protected void AwaitingPoint()
        {
            Assert.Equal(InputType.Point, InputService.AllowedInputTypes);
        }

        protected void Complete()
        {
            InputService.PushNone();
            WaitForCompletion();
        }

        protected void Cancel()
        {
            InputService.Cancel();
            WaitForCompletion();
        }

        protected void VerifyLayerContains(string layerName, Entity entity)
        {
            var x = Workspace.Drawing.Layers[layerName];
            Assert.True(x.Entities.Any(o => o.EquivalentTo(entity)));
        }

        protected void VerifyLayerDoesNotContain(string layerName, Entity entity)
        {
            var x = Workspace.Drawing.Layers[layerName];
            Assert.False(x.Entities.Any(o => o.EquivalentTo(entity)));
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
