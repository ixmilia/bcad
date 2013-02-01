using System.Collections.Generic;
using System.IO;
using BCad.Entities;

namespace BCad.FileHandlers
{
    public interface IFilePlotter
    {
        void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream);
    }
}
