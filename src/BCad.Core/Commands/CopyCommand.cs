// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;
using BCad.Utilities;
using System.Collections.Generic;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Copy", "COPY", ModifierKeys.Control, Key.C, "copy", "co")]
    internal class CopyCommand : AbstractCopyMoveCommand
    {
        protected override Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta)
        {
            foreach (var ent in entities)
            {
                var layer = drawing.ContainingLayer(ent);
                drawing = drawing.Add(layer, EditUtilities.Move(ent, delta));
            }

            return drawing;
        }
    }
}
