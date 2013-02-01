using BCad.Entities;
using BCad.FileHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BCad.Commands.FileHandlers
{
    [ExportFilePlotter(SvgFilePlotter.DisplayName, SvgFilePlotter.FileExtension)]
    internal class SvgFilePlotter : IFilePlotter
    {
        public const string DisplayName = "SVG Files (" + FileExtension + ")";
        public const string FileExtension = ".svg";

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
        }
    }
}
