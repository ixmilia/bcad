using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iegs.Directory;
using BCad.Iegs.Parameter;

namespace BCad.Iegs.Entities
{
    public abstract class IegsEntity
    {
        public abstract IegsEntityType Type { get; }

        public IegsColorNumber Color { get; set; }

        public IegsTransformationMatrix TransformationMatrix { get; set; }

        public static IegsEntity CreateEntity(IegsParameterData parameterData, IegsDirectoryData directoryData, Dictionary<int, IegsTransformationMatrix> transformationMatricies)
        {
            var entity = parameterData.ToEntity(directoryData);
            entity.Color = directoryData.Color;
            if (directoryData.TransformationMatrixPointer != 0)
            {
                entity.TransformationMatrix = transformationMatricies[directoryData.TransformationMatrixPointer];
            }

            return entity;
        }
    }
}
