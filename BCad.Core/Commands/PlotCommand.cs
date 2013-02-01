using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using BCad.FileHandlers;
using BCad.Helpers;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "plot")]
    public class PlotCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

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
                filename = UIHelper.GetFilenameFromUserForOpen(FilePlotters.Select(f => new FileSpecification(f.Metadata.DisplayName, f.Metadata.FileExtensions)));
                if (filename == null)
                    return false;
            }

            var extension = Path.GetExtension(filename);
            var plotter = PlotterFromExtension(extension);
            if (plotter == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            // TODO: prompt for viewport, width, and height
            var viewPort = Workspace.ActiveViewPort;
            var width = 1.0;
            var height = 1.0;

            using (var file = new FileStream(filename, FileMode.Open))
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
