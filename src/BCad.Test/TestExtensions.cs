// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using Xunit;

namespace BCad.Test
{
    public static class TestExtensions
    {
        public static Layer GetLayer(this IWorkspace workspace, string layerName)
        {
            return workspace.Drawing.Layers.GetValue(layerName);
        }

        public static void AddLayer(this IWorkspace workspace, string layerName)
        {
            workspace.Add(new Layer(layerName, null));
        }

        public static void VerifyContains(this Layer layer, Entity entity)
        {
            Assert.True(layer.GetEntities().Any(o => o.EquivalentTo(entity)));
        }

        public static IEnumerable<Entity> GetEntities(this IWorkspace workspace)
        {
            foreach (var layer in workspace.Drawing.GetLayers().OrderBy(l => l.Name))
            {
                foreach (var obj in layer.GetEntities())
                {
                    yield return obj;
                }
            }
        }
    }
}
