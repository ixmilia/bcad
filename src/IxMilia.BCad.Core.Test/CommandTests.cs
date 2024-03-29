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
        public async Task DrawCircleWithPointForRadius()
        {
            var result = await Execute("CIRCLE",
                new PushPointOperation(new Point(1.0, 2.0, 0.0)), // center point
                new PushPointOperation(new Point(2.0, 2.0, 0.0)) // point on radius
            );
            Assert.True(result);
            var actual = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Circle(new Point(1.0, 2.0, 0.0), 1.0, Vector.ZAxis)));
        }

        [Fact]
        public async Task DrawCircleWithDoubleForRadius()
        {
            var result = await Execute("CIRCLE",
                new PushPointOperation(new Point(1.0, 2.0, 0.0)), // center point
                new PushDistanceOperation(1.0) // radius
            );
            Assert.True(result);
            var actual = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Circle(new Point(1.0, 2.0, 0.0), 1.0, Vector.ZAxis)));
        }

        [Fact]
        public async Task DrawCircleWithDiameter()
        {
            var result = await Execute("CIRCLE",
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // initial point
                new PushDirectiveOperation("d"), // switch to diameter mode
                new PushPointOperation(new Point(2.0, 0.0, 0.0)) // second point
            );
            Assert.True(result);
            var actual = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(actual.EquivalentTo(new Circle(new Point(1.0, 0.0, 0.0), 1.0, Vector.ZAxis)));
        }

        [Fact]
        public async Task CopyCommandCanPlaceResultMultipleTimes()
        {
            var line = new Line(new Point(0.0, 0.0, 0.0), new Point(1, 0, 0));
            Workspace.AddToCurrentLayer(line);
            var result = await Execute("COPY",
                new PushEntitiesOperation(new[] { line }), // select line
                new PushNoneOperation(), // done selecting entities
                new PushPointOperation(new Point(5.0, 0.0, 0.0)), // copy base point
                new PushPointOperation(new Point(5.0, 1.0, 0.0)), // insertion point 1
                new PushPointOperation(new Point(5.0, 2.0, 0.0)), // insertion point 2
                new PushNoneOperation() // done
            );
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var lines = Workspace.Drawing.GetEntities().Cast<Line>().ToArray();
            Assert.Equal(3, lines.Length);
            Assert.Equal(new Point(0.0, 0.0, 0.0), lines[0].P1);
            Assert.Equal(new Point(1.0, 0.0, 0.0), lines[0].P2);
            Assert.Equal(new Point(0.0, 1.0, 0.0), lines[1].P1);
            Assert.Equal(new Point(1.0, 1.0, 0.0), lines[1].P2);
            Assert.Equal(new Point(0.0, 2.0, 0.0), lines[2].P1);
            Assert.Equal(new Point(1.0, 2.0, 0.0), lines[2].P2);
        }

        [Fact]
        public async Task FilletCommandReadsRadiusFromDrawingSettings()
        {
            var line1 = new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0));
            var line2 = new Line(new Point(2.0, 0.0, 0.0), new Point(2.0, 2.0, 0.0));
            Assert.NotEqual(0.25, Workspace.Drawing.Settings.FilletRadius);
            Workspace.AddToCurrentLayer(line1);
            Workspace.AddToCurrentLayer(line2);
            Workspace.Update(drawing: Workspace.Drawing.Update(settings: Workspace.Drawing.Settings.Update(filletRadius: 0.25)));
            var result = await Execute("FILLET",
                new PushEntityOperation(new SelectedEntity(line1, new Point(0.5, 0.0, 0.0))),
                new PushEntityOperation(new SelectedEntity(line2, new Point(2.0, 0.5, 0.0)))
            );
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var arc = Workspace.Drawing.GetEntities().OfType<Arc>().Single();
            Assert.Equal(0.25, arc.Radius);
        }

        [Fact]
        public async Task FilletCommandSavesRadiusToDrawingSettings()
        {
            var line1 = new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0));
            var line2 = new Line(new Point(2.0, 0.0, 0.0), new Point(2.0, 2.0, 0.0));
            Assert.NotEqual(0.25, Workspace.Drawing.Settings.FilletRadius);
            Workspace.AddToCurrentLayer(line1);
            Workspace.AddToCurrentLayer(line2);
            var result = await Execute("FILLET",
                new PushEntityOperation(new SelectedEntity(line1, new Point(0.5, 0.0, 0.0))),
                new PushDirectiveOperation("r"),
                new PushDistanceOperation(0.25),
                new PushEntityOperation(new SelectedEntity(line2, new Point(2.0, 0.5, 0.0)))
            );
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var arc = Workspace.Drawing.GetEntities().OfType<Arc>().Single();
            Assert.Equal(0.25, arc.Radius);
            Assert.Equal(0.25, Workspace.Drawing.Settings.FilletRadius);
        }

        [Fact]
        public void SplitLineIntoTokenPartsGeneratesSingleEmptyToken()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("", out var tokens));
            Assert.Equal(new[] { "" }, tokens);
        }

        [Fact]
        public void SplitLineIntoTokenParts()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@" part1 ""part 2""   3   4.0 ", out var tokens));
            Assert.Equal(new[] { "part1", "part 2", "3", "4.0" }, tokens);
        }

        [Fact]
        public void SplitLineIntoTokenPartsNonStringAtEnd()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("a b", out var tokens));
            Assert.Equal(new[] { "a", "b" }, tokens);
        }

        [Fact]
        public void SplitLineIntoTokenPartsStringAtEnd()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@"a ""b""", out var tokens));
            Assert.Equal(new[] { "a", "b" }, tokens);
        }

        [Fact]
        public void SplitLineIntoTokenPartsBackslashDoesNotEscapeInString()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(@" ""C:\path\to\file"" ", out var tokens));
            Assert.Equal(new[] { @"C:\path\to\file" }, tokens);
        }

        [Fact]
        public void SplitLineFailsOnUnterminatedString()
        {
            Assert.False(CommandLineSplitter.TrySplitIntoTokens(@"a ""b", out var _));
        }

        [Fact]
        public void SplitLineIntoTokenPartsIgnoresCommentLines()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("; comment at start of line\n; comment at start of another line", out var tokens));
            Assert.Empty(tokens);
        }

        [Fact]
        public void SplitLineIntoTokenPartsReturnsTokenAfterCommentLine()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("; some comment\nabc", out var tokens));
            Assert.Equal(new[] { "abc" }, tokens);
        }

        [Fact]
        public void SplitLineIntoTokenPartsTreatsSemicolonNotAtStartAsToken()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens(" ;a-token", out var tokens));
            Assert.Equal(new[] { ";a-token" }, tokens);
        }

        [Fact]
        public void SplitScriptIntoTokenPartsWithNewlines()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("a b c\n   d e   ", out var tokens));
            Assert.Equal(new[] { "a", "b", "c", "d", "e" }, tokens);
        }

        [Fact]
        public void SplitScriptIntoTokenPartsAlwaysProducesEmptyTokenFromEmptyLine()
        {
            Assert.True(CommandLineSplitter.TrySplitIntoTokens("a\n\nb", out var tokens));
            Assert.Equal(new[] { "a", "", "b" }, tokens);
        }

        [Fact]
        public async Task InsertLineSegments()
        {
            var result = await Workspace.ExecuteTokensFromScriptAsync("LINE 0,0 1,0\n");
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
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"LINE 0,0 1,0 0,1 c");
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
        public async Task InsertLineSegments_PointToTangent()
        {
            var circle = new Circle(new Point(1.0, 0.0, 0.0), 0.5, Vector.ZAxis);
            Workspace.AddToCurrentLayer(circle);
            var result = await Execute("LINE",
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // start point
                new PushDirectiveOperation("t"), // switch to tangent mode
                new PushEntityOperation(new SelectedEntity(circle, new Point(0.74, 0.43, 0.0))), // the selection point
                new PushNoneOperation() // done
            );
            Assert.True(result);
            var actual = Workspace.Drawing.GetEntities().OfType<Line>().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.0, 0.0, 0.0), new Point(0.75, 0.4330127018922193, 0.0))));
        }

        [Fact]
        public async Task InsertLineSegments_TangentToPoint()
        {
            var circle = new Circle(new Point(1.0, 0.0, 0.0), 0.5, Vector.ZAxis);
            Workspace.AddToCurrentLayer(circle);
            var result = await Execute("LINE",
                new PushDirectiveOperation("t"),  // enter tangent mode
                new PushEntityOperation(new SelectedEntity(circle, new Point(0.74, 0.43, 0.0))), // the entity
                new PushPointOperation(new Point(0.0, 0.0, 0.0)), // the point
                new PushNoneOperation() // done
            );
            Assert.True(result);
            var actual = Workspace.Drawing.GetEntities().OfType<Line>().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(0.75, 0.4330127018922193, 0.0), new Point(0.0, 0.0, 0.0))));
        }

        [Fact]
        public async Task InsertLineSegments_TangentToTangent()
        {
            var circle1 = new Circle(new Point(0.0, 0.0, 0.0), 1.0, Vector.ZAxis);
            var circle2 = new Circle(new Point(3.0, 0.0, 0.0), 1.5, Vector.ZAxis);
            Workspace.AddToCurrentLayer(circle1);
            Workspace.AddToCurrentLayer(circle2);
            var result = await Execute("LINE",
                new PushDirectiveOperation("t"),  // enter tangent mode
                new PushEntityOperation(new SelectedEntity(circle1, new Point(-0.15, 0.98, 0.0))), // first circle
                new PushDirectiveOperation("t"), // remain in tangent mode
                new PushEntityOperation(new SelectedEntity(circle2, new Point(2.75, 1.48, 0.0))), // second circle
                new PushNoneOperation() // done
            );
            Assert.True(result);
            var actual = Workspace.Drawing.GetEntities().OfType<Line>().Single();
            Assert.True(actual.EquivalentTo(new Line(new Point(-0.1666666666674352, 0.9860132971831395, 0.0), new Point(2.7499999999985647, 1.4790199457769997, 0.0))));
        }

        [Fact]
        public async Task InsertCircleWithTwoPointsScript()
        {
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"CIRCLE 1,0 2,0");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var circle = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(circle.EquivalentTo(new Circle(new Point(1.0, 0.0, 0.0), 1.0, Vector.ZAxis)));
        }

        [Fact]
        public async Task InsertCircleWithPointAndDistanceScript()
        {
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"CIRCLE 1,0 1");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var circle = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(circle.EquivalentTo(new Circle(new Point(1.0, 0.0, 0.0), 1.0, Vector.ZAxis)));
        }

        [Fact]
        public async Task InsertCircleWithDiameterScript()
        {
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"CIRCLE 1,0 d 2,0");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var circle = (Circle)Workspace.Drawing.GetEntities().Single();
            Assert.True(circle.EquivalentTo(new Circle(new Point(1.5, 0.0, 0.0), 0.5, Vector.ZAxis)));
        }

        [Fact]
        public async Task InsertTextFromCommandLine()
        {
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"TEXT 1,2 3 test");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var actual = (Text)Workspace.Drawing.GetEntities().Single();
            Assert.Equal(new Point(1.0, 2.0, 0.0), actual.Location);
            Assert.Equal(3.0, actual.Height);
            Assert.Equal("test", actual.Value);
        }

        [Fact]
        public async Task DrawEllipseCorrectsForMinorAxisRatio()
        {
            // ellipse minor axis ratio should be (0, 1); when drawn incorrectly, normalize the entity
            var result = await Workspace.ExecuteTokensFromScriptAsync(@"ELLIPSE 0,0 1,0 0,2");
            Assert.True(result);
            Assert.False(Workspace.IsCommandExecuting);
            var actual = (Ellipse)Workspace.Drawing.GetEntities().Single();
            Assert.Equal(new Point(0.0, 0.0, 0.0), actual.Center);
            Assert.Equal(new Vector(0.0, 2.0, 0.0), actual.MajorAxis);
            Assert.Equal(0.5, actual.MinorAxisRatio);
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
