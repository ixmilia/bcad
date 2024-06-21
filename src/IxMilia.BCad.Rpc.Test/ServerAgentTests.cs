using System;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Lisp.Test;
using Xunit;

namespace IxMilia.BCad.Rpc.Test
{
    public class ServerAgentTests
    {
        [Fact]
        public void SelectedEntitiesAreUpdatedWhenSettingAProperty()
        {
            var sa = new ServerAgent(new TestLispWorkspace(), null);

            // add entity and select it
            var originalEntity = new Location(new Point(1.0, 1.0, 1.0));
            var untouchedEntity = new Location(new Point(2.0, 2.0, 2.0));
            sa.Workspace.Add(sa.Workspace.Drawing.GetLayers().Single(), originalEntity);
            sa.Workspace.Add(sa.Workspace.Drawing.GetLayers().Single(), untouchedEntity);
            sa.Workspace.SelectedEntities.Add(originalEntity); // selected=(1,1,1), unselected=(2,2,2)

            // update the entity
            sa.SetPropertyPaneValue(new ClientPropertyPaneValue("x", "displayName", "9"));

            // verify that the new entity is selected and unselected entity is still selected
            Assert.Equal(new Point(9.0, 1.0, 1.0), ((Location)sa.Workspace.SelectedEntities.Single()).Point);
        }

        [Fact]
        public void MultipleEntitiesAreUpdatedTogetherWhenSettingAProperty()
        {
            var sa = new ServerAgent(new TestLispWorkspace(), null);

            // add a test layer
            sa.Workspace.Add(new Layer("test-layer"));

            // add 2 entities to layer 0
            var e1 = new Location(new Point());
            var e2 = new Location(new Point());
            sa.Workspace.AddToCurrentLayer(e1);
            sa.Workspace.AddToCurrentLayer(e2);
            Assert.Equal("0", sa.Workspace.Drawing.ContainingLayer(e1).Name);
            Assert.Equal("0", sa.Workspace.Drawing.ContainingLayer(e2).Name);

            // select entities and update the layer
            sa.Workspace.SelectedEntities.Set(new[] { e1, e2 });
            sa.SetPropertyPaneValue(new ClientPropertyPaneValue("layer", "displayName", "test-layer"));

            // verify new layer
            Assert.Equal("test-layer", sa.Workspace.Drawing.ContainingLayer(e1).Name);
            Assert.Equal("test-layer", sa.Workspace.Drawing.ContainingLayer(e2).Name);
        }

        [Fact]
        public async Task NormalInputIsProcessedOnSpaceAsync()
        {
            var sa = new ServerAgent(new TestLispWorkspace(), null);

            // add a test line
            var line = new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0));
            sa.Workspace.AddToCurrentLayer(line);

            var inputRequestNumber = 0;
            sa.Workspace.InputService.ValueRequested += (s, e) =>
            {
                switch (inputRequestNumber)
                {
                    case 0:
                        // entities
                        sa.Workspace.InputService.PushEntities(new[] { line });
                        break;
                    case 1:
                        // done with entities
                        sa.Workspace.InputService.PushNone();
                        break;
                    case 2:
                        // base point
                        sa.Workspace.InputService.PushPoint(new Point(0.0, 0.0, 0.0));
                        break;
                    case 3:
                        // scale factor
                        var inputText = "2 ";
                        for (int i = 1; i <= inputText.Length; i++)
                        {
                            sa.InputChanged(inputText.Substring(0, i)).Wait();
                        }
                        break;
                    default:
                        throw new Exception("Unexpected request for input");
                }

                inputRequestNumber++;
            };

            // scale the line
            var scaleCommandTask = sa.Workspace.ExecuteCommand("scale");
            var timeoutTask = Task.Delay(1000);
            var completedTask = await Task.WhenAny(scaleCommandTask, timeoutTask);
            if (ReferenceEquals(completedTask, timeoutTask))
            {
                throw new Exception("Expected command completion, got timeout");
            }

            // verify the line was scaled
            var scaledLine = (Line)sa.Workspace.Drawing.GetEntities().Single();
            Assert.Equal(new Point(2.0, 2.0, 0.0), scaledLine.P2);
        }

        [Fact]
        public async Task LispLikeInputIsNotProcessedOnSpaceAsync()
        {
            var sa = new ServerAgent(new TestLispWorkspace(), null);

            // add a test line
            var line = new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0));
            sa.Workspace.AddToCurrentLayer(line);

            var inputRequestNumber = 0;
            var commandCompleted = false;
            sa.Workspace.CommandExecuted += (s, e) =>
            {
                commandCompleted = true;
            };
            sa.Workspace.InputService.ValueRequested += (s, e) =>
            {
                switch (inputRequestNumber)
                {
                    case 0:
                        // entities
                        sa.Workspace.InputService.PushEntities(new[] { line });
                        break;
                    case 1:
                        // done with entities
                        sa.Workspace.InputService.PushNone();
                        break;
                    case 2:
                        // base point
                        sa.Workspace.InputService.PushPoint(new Point(0.0, 0.0, 0.0));
                        break;
                    case 3:
                        // scale factor
                        var inputText = "(+ 1 1)";
                        for (int i = 1; i <= inputText.Length; i++)
                        {
                            sa.InputChanged(inputText.Substring(0, i)).Wait();
                            if (commandCompleted && i < inputText.Length)
                            {
                                throw new Exception("Command completed early");
                            }
                        }

                        // simulate the user pressing <Enter>
                        sa.SubmitInput(inputText).Wait();
                        break;
                    default:
                        throw new Exception("Unexpected request for input");
                }

                inputRequestNumber++;
            };

            // scale the line
            var scaleCommandTask = sa.Workspace.ExecuteCommand("scale");
            var timeoutTask = Task.Delay(1000);
            var completedTask = await Task.WhenAny(scaleCommandTask, timeoutTask);
            if (ReferenceEquals(completedTask, timeoutTask))
            {
                throw new Exception("Expected command completion, got timeout");
            }

            var commandResult = await scaleCommandTask;
            if (!commandResult)
            {
                throw new Exception("Command failed");
            }

            // verify the line was scaled
            var scaledLine = (Line)sa.Workspace.Drawing.GetEntities().Single();
            Assert.Equal(new Point(2.0, 2.0, 0.0), scaledLine.P2);
        }
    }
}
