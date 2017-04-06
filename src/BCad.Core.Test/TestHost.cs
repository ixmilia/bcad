// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;

namespace BCad.Core.Test
{
    public class TestHost : IDisposable
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        private CompositionHost container;

        private TestHost()
        {
            var configuration = new ContainerConfiguration()
                .WithAssemblies(new[]
                {
                    typeof(TestHost).GetTypeInfo().Assembly, // this assembly
                    typeof(Drawing).GetTypeInfo().Assembly // BCad.Core.dll
                });
            container = configuration.CreateContainer();
            container.SatisfyImports(this);
            Workspace.Update(drawing: new Drawing());
        }

        public static TestHost CreateHost()
        {
            return new TestHost();
        }

        public static TestHost CreateHost(params string[] layerNames)
        {
            var host = CreateHost();
            foreach (string layer in layerNames)
                host.Workspace.Add(new Layer(layer, null));
            return host;
        }

        public void Dispose()
        {
            if (container != null)
            {
                container.Dispose();
                container = null;
            }
        }
    }
}
