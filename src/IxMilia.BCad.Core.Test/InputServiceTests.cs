using System;
using System.Threading.Tasks;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class InputServiceTests : TestBase
    {
        [Fact]
        public async Task GetDistanceFromTwoPoints()
        {
            var result = await DoOperations(
                () => InputService.GetDistance(),
                new PushPointOperation(new Point(1.0, 2.0, 0.0)),
                new PushPointOperation(new Point(2.0, 2.0, 0.0))
            );
            Assert.Equal(1.0, result.Value);
        }

        [Fact]
        public async Task GetDistanceFromOnePointAfterStartPointGiven()
        {
            var result = await DoOperations(
                () => InputService.GetDistanceFromPoint(new Point(1.0, 2.0, 0.0)),
                new PushPointOperation(new Point(2.0, 2.0, 0.0))
            );
            Assert.Equal(1.0, result.Value);
        }

        [Fact]
        public async Task GetDistanceFromDouble()
        {
            var result = await DoOperations(
                () => InputService.GetDistance(),
                new PushDistanceOperation(1.0)
            );
            Assert.Equal(1.0, result.Value);
        }

        [Fact]
        public async Task GetDistanceFromDoubleAfterStartPointGiven()
        {
            var result = await DoOperations(
                () => InputService.GetDistanceFromPoint(new Point(1.0, 2.0, 0.0)),
                new PushDistanceOperation(1.0)
            );
            Assert.Equal(1.0, result.Value);
        }

        [Fact]
        public async Task GetDistanceFromDoubleAfterAPointWasPushed()
        {
            var result = await DoOperations(
                () => InputService.GetDistance(),
                new PushPointOperation(new Point(100.0, 200.0, 0.0)),
                new PushDistanceOperation(1.0)
            );
            Assert.Equal(1.0, result.Value);
        }

        protected async Task<ValueOrDirective<T>> DoOperations<T>(Func<Task<ValueOrDirective<T>>> requestingAction, params TestWorkspaceOperation[] workspaceOperations)
        {
            var operationIndex = 0;
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

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var requestTask = requestingAction();
            var completedTask = await Task.WhenAny(requestTask, timeoutTask);
            if (ReferenceEquals(completedTask, timeoutTask))
            {
                throw new Exception("Command execution timed out");
            }

            if (operationIndex < workspaceOperations.Length)
            {
                throw new Exception("Unprocessed operations remain on the stack");
            }

            var result = await requestTask;
            return result;
        }
    }
}
