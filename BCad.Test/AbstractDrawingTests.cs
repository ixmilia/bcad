using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BCad.Entities;
using BCad.Extensions;
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
        protected IInputService InputService { get { return Host.InputService; } }
        protected IEditService EditService { get { return Host.EditService; } }

        protected void AssertClose(double expected, double actual)
        {
            Assert.True(Math.Abs(expected - actual) < MathHelper.Epsilon, string.Format("Expected: {0}\nActual: {1}", expected, actual));
        }

        protected void AssertPointClose(Point expected, Point actual)
        {
            AssertClose(expected.X, actual.X);
            AssertClose(expected.Y, actual.Y);
            AssertClose(expected.Z, actual.Z);
        }
    }
}
