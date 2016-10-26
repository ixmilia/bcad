// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using BCad.Entities;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Extend", "EXTEND", "extend", "ex")]
    public class ExtendCommand : AbstractTrimExtendCommand
    {
        protected override string GetBoundsText()
        {
            return "Select bounding edges";
        }

        protected override string GetTrimExtendText()
        {
            return "Entity to extend";
        }

        protected override void DoTrimExtend(SelectedEntity selectedEntity, IEnumerable<Primitives.IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            EditUtilities.Extend(selectedEntity, boundaryPrimitives, out removed, out added);
        }
    }
}
