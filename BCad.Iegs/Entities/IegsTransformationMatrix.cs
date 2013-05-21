using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Iegs.Entities
{
    public class IegsTransformationMatrix : IegsEntity
    {
        public override IegsEntityType Type { get { return IegsEntityType.TransformationMatrix; } }

        // TODO: add matrix data

        public IegsTransformationMatrix()
        {
        }

        public IegsPoint Transform(IegsPoint point)
        {
            throw new NotImplementedException();
        }
    }
}
