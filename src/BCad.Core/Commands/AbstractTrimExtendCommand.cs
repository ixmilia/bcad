// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Commands
{
    public abstract class AbstractTrimExtendCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var boundaries = await workspace.InputService.GetEntities(GetBoundsText());
            if (boundaries.Cancel || !boundaries.HasValue || !boundaries.Value.Any())
            {
                return false;
            }

            var boundaryPrimitives = boundaries.Value.SelectMany(b => b.GetPrimitives());

            var directive = new UserDirective(GetTrimExtendText());
            var selected = await workspace.InputService.GetEntity(directive);
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            string entityLayerName;
            while (!selected.Cancel && selected.HasValue)
            {
                var drawing = workspace.Drawing;
                entityLayerName = drawing.ContainingLayer(selected.Value.Entity).Name;
                DoTrimExtend(selected.Value, boundaryPrimitives, out removed, out added);

                foreach (var ent in removed)
                    drawing = drawing.Remove(ent);

                foreach (var ent in added)
                    drawing = drawing.Add(drawing.Layers.GetValue(entityLayerName), ent);

                // commit the change
                if (workspace.Drawing != drawing)
                    workspace.Update(drawing: drawing);

                // get next entity to trim/extend
                selected = await workspace.InputService.GetEntity(directive);
            }

            return true;
        }

        protected abstract string GetBoundsText();
        protected abstract string GetTrimExtendText();
        protected abstract void DoTrimExtend(SelectedEntity selectedEntity, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added);
    }
}
