using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iges.Directory;
using BCad.Iges.Parameter;

namespace BCad.Iges.Entities
{
    public abstract class IgesEntity
    {
        public abstract IgesEntityType Type { get; }

        public IgesColorNumber Color { get; set; }

        public IgesTransformationMatrix TransformationMatrix { get; set; }

        public int Form { get; internal set; }

        public abstract int LineCount { get; }

        internal static IgesEntity CreateEntity(IgesParameterData parameterData, IgesDirectoryData directoryData, Dictionary<int, IgesTransformationMatrix> transformationMatricies)
        {
            var entity = parameterData.ToEntity(directoryData);
            entity.Color = directoryData.Color;
            if (directoryData.TransformationMatrixPointer != 0)
            {
                entity.TransformationMatrix = transformationMatricies[directoryData.TransformationMatrixPointer];
            }
            else
            {
                entity.TransformationMatrix = IgesTransformationMatrix.Identity;
            }

            return entity;
        }
    }
}
