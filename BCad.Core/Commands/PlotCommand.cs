using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
using BCad.FileHandlers;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "plot")]
    public class PlotCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [ImportMany]
        private IEnumerable<Lazy<IFilePlotter, IFilePlotterMetadata>> FilePlotters = null;

        public bool Execute(object arg)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "PLOT"; }
        }
    }
}
