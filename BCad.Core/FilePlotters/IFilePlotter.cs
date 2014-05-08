using System.Collections.Generic;
using System.IO;
using BCad.Entities;

namespace BCad.FilePlotters
{
    public interface IFilePlotter
    {
        void Plot(IEnumerable<ProjectedEntity> entities, ColorMap colorMap, double width, double height, Stream stream);
    }
}
