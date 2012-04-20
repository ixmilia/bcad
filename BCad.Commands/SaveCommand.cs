using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BCad.FileHandlers;

namespace BCad.Commands
{
    [ExportCommand("File.Save", ModifierKeys.Control, Key.S, "save", "s")]
    internal class SaveCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [ImportMany]
        private IEnumerable<IFileWriter> FileWriters = null;

        public bool Execute(params object[] param)
        {
            return SaveAsCommand.Execute(Workspace, FileWriters, Workspace.Document.FileName);
        }

        public string DisplayName
        {
            get { return "SAVE"; }
        }
    }
}
