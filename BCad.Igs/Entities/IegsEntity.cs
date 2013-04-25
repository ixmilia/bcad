using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Igs.Entities
{
    public abstract class IegsEntity
    {
        public abstract IegsEntityType Type { get; }

        public IegsColorNumber Color { get; set; }
    }
}
