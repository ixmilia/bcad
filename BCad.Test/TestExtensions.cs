using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad;
using BCad.Extensions;
using BCad.Objects;
using Xunit;

namespace BCad.Test
{
    public static class TestExtensions
    {
        public static Layer GetLayer(this IWorkspace workspace, string layerName)
        {
            return workspace.Document.Layers[layerName];
        }

        public static void AddLayer(this IWorkspace workspace, string layerName)
        {
            workspace.Add(new Layer(layerName, Color.Auto));
        }

        public static void VerifyContains(this Layer layer, IObject obj)
        {
            Assert.True(layer.Objects.Any(o => o.EquivalentTo(obj)));
        }
    }
}
