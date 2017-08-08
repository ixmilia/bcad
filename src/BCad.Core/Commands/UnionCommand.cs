// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Edit.Union", "UNION", "union", "un")]
    public class UnionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<Entity> Combine(IEnumerable<Entity> entities)
        {
            return entities.Union();
        }
    }
}
