using System;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Services;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class CommandTests : TestBase
    {
        [Fact]
        public async Task RotateWithAngle()
        {
            var line = new Line(new Point(0, 0, 0), new Point(1, 0, 0));
            Workspace.AddToCurrentLayer(line);
            var result = await Execute("ROTATE",
                new PushEntitiesOperation(new[] { line }), // select the line
                new PushNoneOperation(), // done selecting entities
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // base point
                new PushTextOperation("90") // rotation angle
            );
            Assert.True(result);
            var actual = (Line)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0))));
        }

        [Fact]
        public async Task RotateWithReference()
        {
            var line = new Line(new Point(0, 0, 0), new Point(1, 0, 0));
            Workspace.AddToCurrentLayer(line);
            var result = await Execute("ROTATE",
                new PushEntitiesOperation(new[] { line }), // select the line
                new PushNoneOperation(), // done selecting entities
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // base point
                new PushTextOperation("r"), // rotate by reference
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // base point
                new PushPointOperation(new Point(1.0, 0.0, 0.0)), // leg 1
                new PushPointOperation(new Point(0.0, 1.0, 0.0)) // leg 2
            );
            Assert.True(result);
            var actual = (Line)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0))));
        }

        [Fact]
        public async Task ScaleWithDistance()
        {
            var line = new Line(new Point(0, 0, 0), new Point(1, 0, 0));
            Workspace.AddToCurrentLayer(line);
            var result = await Execute("SCALE",
                new PushEntitiesOperation(new[] { line }), // select the line
                new PushNoneOperation(), // done selecting entities
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // base point
                new PushDistanceOperation(2.0) // scale factor
            );
            Assert.True(result);
            var actual = (Line)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0))));
        }

        [Fact]
        public async Task ScaleWithReference()
        {
            var line = new Line(new Point(0, 0, 0), new Point(1, 0, 0));
            Workspace.AddToCurrentLayer(line);
            var result = await Execute("SCALE",
                new PushEntitiesOperation(new[] { line }), // select the line
                new PushNoneOperation(), // done selecting entities
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // base point
                new PushDirectiveOperation("r"), // scale by reference
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // first scale reference point
                new PushPointOperation(new Point(1.0, 0.0, 0.0)), // first scale value point
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // second scale reference point
                new PushPointOperation(new Point(0.0, 2.0, 0.0)) // second scale value point
            );
            Assert.True(result);
            var actual = (Line)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0))));
        }

        protected async Task<bool> Execute(string command, params TestWorkspaceOperation[] workspaceOperations)
        {
            var operationIndex = 0;
            Workspace.CommandExecuted += (sender, e) =>
            {
                if (operationIndex < workspaceOperations.Length)
                {
                    throw new Exception($"Operations remain on the stack: {string.Join(", ", workspaceOperations.Skip(operationIndex))}");
                }
            };

            Workspace.InputService.ValueRequested += (sender, e) =>
            {
                if (operationIndex >= workspaceOperations.Length)
                {
                    throw new Exception("No operations remain on the stack");
                }

                var operation = workspaceOperations[operationIndex];
                operationIndex++;
                if ((e.InputType & operation.InputType) == operation.InputType)
                {
                    switch (operation.InputType)
                    {
                        case InputType.None:
                            Workspace.InputService.PushNone();
                            break;
                        case InputType.Command:
                            Workspace.InputService.PushCommand(((PushCommandOperation)operation).Command);
                            break;
                        case InputType.Distance:
                            Workspace.InputService.PushDistance(((PushDistanceOperation)operation).Distance);
                            break;
                        case InputType.Directive:
                            Workspace.InputService.PushDirective(((PushDirectiveOperation)operation).Directive);
                            break;
                        case InputType.Point:
                            Workspace.InputService.PushPoint(((PushPointOperation)operation).Point);
                            break;
                        case InputType.Entity:
                            Workspace.InputService.PushEntity(((PushEntityOperation)operation).Entity);
                            break;
                        case InputType.Entities:
                            Workspace.InputService.PushEntities(((PushEntitiesOperation)operation).Entities);
                            break;
                        case InputType.Text:
                            Workspace.InputService.PushText(((PushTextOperation)operation).Text);
                            break;
                        default:
                            throw new Exception($"Unexpected workspace operation type {operation.InputType}");
                    }
                }
            };

            var executeTask = Workspace.ExecuteCommand(command);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(executeTask, timeoutTask);
            if (ReferenceEquals(completedTask, timeoutTask))
            {
                throw new Exception("Command execution timed out");
            }

            var result = await executeTask;
            return result;
        }
    }
}
