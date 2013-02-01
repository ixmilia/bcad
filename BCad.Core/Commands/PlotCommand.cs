using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using BCad.FileHandlers;
using BCad.Helpers;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "plot")]
    public class PlotCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IInputService InputService = null;

        [Import]
        private IExportService ExportService = null;

        [ImportMany]
        private IEnumerable<Lazy<IFilePlotter, IFilePlotterMetadata>> FilePlotters = null;

        public bool Execute(object arg)
        {
            string filename = null;
            if (arg is string)
                filename = (string)arg;
            if (filename == null)
            {
                filename = UIHelper.GetFilenameFromUserForSave(FilePlotters.Select(f => new FileSpecification(f.Metadata.DisplayName, f.Metadata.FileExtensions)));
                if (filename == null)
                    return false;
            }

            var extension = Path.GetExtension(filename);
            var plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            // TODO: generalize getting viewports for zoom, etc.
            // prompt for viewport
            var firstPoint = InputService.GetPoint(new UserDirective("First corner of view box"));
            if (firstPoint.Cancel || !firstPoint.HasValue)
                return false;

            var secondPoint = InputService.GetPoint(new UserDirective("Second corner of view box"), (p) =>
                {
                    var a = firstPoint.Value;
                    var b = new Point(p.X, firstPoint.Value.Y, firstPoint.Value.Z);
                    var c = new Point(p.X, p.Y, firstPoint.Value.Z);
                    var d = new Point(firstPoint.Value.X, p.Y, firstPoint.Value.Z);
                    return new[]
                    {
                        new PrimitiveLine(a, b),
                        new PrimitiveLine(b, c),
                        new PrimitiveLine(c, d),
                        new PrimitiveLine(d, a)
                    };
                });
            if (secondPoint.Cancel || !secondPoint.HasValue)
                return false;

            // prompt for viewport, width, and height
            var size = secondPoint.Value - firstPoint.Value;
            var width = Math.Abs(size.X);
            var height = Math.Abs(size.Y);
            var viewPort = new ViewPort(
                new Point(Math.Min(firstPoint.Value.X, secondPoint.Value.X), Math.Min(firstPoint.Value.Y, secondPoint.Value.Y), firstPoint.Value.Z),
                Workspace.ActiveViewPort.Sight, Workspace.ActiveViewPort.Up, height);

            using (var file = new FileStream(filename, FileMode.Create))
            {
                var entities = ExportService.ProjectTo2D(Workspace.Drawing, viewPort);
                plotter.Plot(entities, width, height, file);
            }

            return true;
        }

        private IFilePlotter PlotterFromExtension(string extension)
        {
            var plotter = FilePlotters.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension));
            if (plotter == null)
                return null;
            return plotter.Value;
        }

        public string DisplayName
        {
            get { return "PLOT"; }
        }
    }
}
