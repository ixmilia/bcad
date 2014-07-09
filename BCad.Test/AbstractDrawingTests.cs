using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        protected void AssertClose(double expected, double actual)
        {
            Assert.IsTrue(Math.Abs(expected - actual) < MathHelper.Epsilon, string.Format("Expected: {0}\nActual: {1}", expected, actual));
        }

        protected void AssertClose(Point expected, Point actual)
        {
            AssertClose(expected.X, actual.X);
            AssertClose(expected.Y, actual.Y);
            AssertClose(expected.Z, actual.Z);
        }
    }
}
