using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    [ExportCommand("Edit.Trim", "trim", "tr", "t")]
    public class TrimCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var boundaries = InputService.GetEntities();
            if (boundaries.Cancel || !boundaries.HasValue || !boundaries.Value.Any())
            {
                return false;
            }

            var directive = new UserDirective("Entity to trim");
            var selected = InputService.GetEntity(directive);
            var doc = Workspace.Document;
            while (!selected.Cancel && selected.HasValue)
            {
                // TODO: perform the trim operation

                selected = InputService.GetEntity(directive);
            }

            Workspace.Document = doc;
            return true;
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
