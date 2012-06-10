using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BCad.Entities;
using BCad.Extensions;
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
        protected ITrimExtendService TrimExtendService { get { return Host.TrimExtendService; } }
    }
}
