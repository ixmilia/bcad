using System;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
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

        [Fact]
        public void SplitLineIntoTokenPartsGeneratesSingleEmptyToken()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("", out var parts));
            Assert.Equal(new[] { "" }, parts);
        }

        [Fact]
        public void SplitLineIntoTokenParts()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@" part1 ""part 2""   3   4.0 ", out var parts));
            Assert.Equal(new[] { "part1", "part 2", "3", "4.0" }, parts);
        }

        [Fact]
        public void SplitLineIntoTokenPartsNonStringAtEnd()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("a b", out var parts));
            Assert.Equal(new[] { "a", "b" }, parts);
        }

        [Fact]
        public void SplitLineIntoTokenPartsStringAtEnd()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@"a ""b""", out var parts));
            Assert.Equal(new[] { "a", "b" }, parts);
        }

        [Fact]
        public void SplitLineIntoTokenPartsBackslashDoesNotEscapeInString()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@" ""C:\path\to\file"" ", out var parts));
            Assert.Equal(new[] { @"C:\path\to\file" }, parts);
        }

        [Fact]
        public void SplitLineFailsOnUnterminatedString()
        {
            Assert.False(CommandLineSplitter.TrySplitIntoTokens(@"a ""b", out var _));
        }

        [Fact]
        public async Task InsertLineSegments()
        {
            var result = await Workspace.ExecuteTokensFromLinesAsync(new[] { "LINE 0,0 1,0", "" });
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var entities = Workspace.Drawing.GetEntities().ToArray();
            var line = (Line)entities.Single();
            Assert.Equal(new Point(0.0, 0.0, 0.0), line.P1);
            Assert.Equal(new Point(1.0, 0.0, 0.0), line.P2);
        }

        [Fact]
        public async Task InsertLineSegmentsWithCloseCommand()
        {
            var result = await Workspace.ExecuteTokensFromLineAsync(@"LINE 0,0 1,0 0,1 c");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var entities = Workspace.Drawing.GetEntities().ToArray();
            Assert.Equal(3, entities.Length);
            var l1 = (Line)entities[0];
            var l2 = (Line)entities[1];
            var l3 = (Line)entities[2];
            Assert.Equal(new Point(0.0, 0.0, 0.0), l1.P1);
            Assert.Equal(new Point(1.0, 0.0, 0.0), l1.P2);
            Assert.Equal(new Point(1.0, 0.0, 0.0), l2.P1);
            Assert.Equal(new Point(0.0, 1.0, 0.0), l2.P2);
            Assert.Equal(new Point(0.0, 1.0, 0.0), l3.P1);
            Assert.Equal(new Point(0.0, 0.0, 0.0), l3.P2);
        }

        [Fact]
        public async Task InsertTextFromCommandLine()
        {
            var result = await Workspace.ExecuteTokensFromLineAsync(@"TEXT 1,2 3 test");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var actual = (Text)Workspace.Drawing.GetEntities().Single();
            Assert.Equal(new Point(1.0, 2.0, 0.0), actual.Location);
            Assert.Equal(3.0, actual.Height);
            Assert.Equal("test", actual.Value);
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
                operation.DoOperation(Workspace);
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
