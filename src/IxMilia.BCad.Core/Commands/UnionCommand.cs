using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Commands
{
    public class UnionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<Entity> Combine(IEnumerable<Entity> entities)
        {
            return entities.Union();
        }
    }
}
