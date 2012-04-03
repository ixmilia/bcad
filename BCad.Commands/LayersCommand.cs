using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;
using BCad.UI;

namespace BCad.Commands
{
    [ExportCommand("Edit.Layers", "layers", "lay", "la")]
    internal class LayersCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDialogFactory DialogFactory { get; set; }

        public bool Execute(params object[] parameters)
        {
            var result = DialogFactory.ShowDialog(Workspace.SettingsManager.LayerDialogId);
            return true;
        }

        public string DisplayName
        {
            get { return "LAYERS"; }
        }
    }
}
