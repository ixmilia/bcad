using System.Collections.Generic;
using BCad.Entities;

namespace BCad.Services
{
    public interface IExportService
    {
        IEnumerable<ProjectedEntity> ProjectTo2D(Drawing drawing, ViewPort viewPort);
    }
}
