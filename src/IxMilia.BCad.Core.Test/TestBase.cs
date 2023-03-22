using System;
using System.Linq;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Services;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public abstract class TestBase
    {
        protected TestBase()
        {
            Host = TestHost.CreateHost();
        }

        protected TestHost Host { get; set; }
        protected IWorkspace Workspace { get { return Host.Workspace; } }
        protected IInputService InputService { get { return Host.Workspace.InputService; } }

        protected void AssertClose(double expected, double actual, double error = MathHelper.Epsilon)
        {
            Assert.True(Math.Abs(expected - actual) < error, string.Format("Expected: {0}\nActual: {1}", expected, actual));
        }

        protected void AssertClose(Point expected, Point actual)
        {
            AssertClose(expected.X, actual.X);
            AssertClose(expected.Y, actual.Y);
            AssertClose(expected.Z, actual.Z);
        }

        protected void AssertContains<T>(T[] expectedSubset, T[] actual) where T : IComparable<T>
        {
            for (int i = 0; i < actual.Length - expectedSubset.Length; i++)
            {
                var candidateSubset = actual.Skip(i).Take(expectedSubset.Length).ToArray();
                var foundMatch = true;
                for (int j = 0; j < expectedSubset.Length; j++)
                {
                    if (expectedSubset[j].CompareTo(candidateSubset[j]) != 0)
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    // found a match, just quit
                    return;
                }
            }

            Assert.Fail($"Unable to find subset {expectedSubset} in {actual}");
        }
    }
}
