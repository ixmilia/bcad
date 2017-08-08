// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
    }
}
