using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad;

namespace BCad.Test
{
    public static class TestExtensions
    {
        public static Layer GetLayer(this IWorkspace workspace, string layerName)
        {
            return workspace.Document.Layers[layerName];
        }
    }
}
