using System;
using BCad.Helpers;
using BCad.Services;
using Xunit;

namespace BCad.Test
{
    public abstract class AbstractDrawingTests
    {
        public AbstractDrawingTests()
        {
            this.Host = TestHost.CreateHost();
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
    }
}
