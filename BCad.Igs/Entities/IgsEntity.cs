using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Igs.Entities
{
    public abstract class IgsEntity
    {
        public abstract IgsEntityType Type { get; }

        public IgsColorNumber Color { get; set; }
    }
}
