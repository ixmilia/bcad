using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad;
using BCad.Entities;
using BCad.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.IsTrue(layer.GetEntities().Any(o => o.EquivalentTo(entity)));
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
