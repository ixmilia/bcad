// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Intersection", "INTERSECTION", "intersection", "int")]
    public class IntersectionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<Entity> Combine(IEnumerable<Entity> entities)
        {
            return entities.Intersect();
        }
    }
}
