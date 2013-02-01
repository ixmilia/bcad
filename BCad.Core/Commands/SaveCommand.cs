using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BCad.FileHandlers;

namespace BCad.Commands
{
    [ExportCommand("File.Save", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [ImportMany]
        private IEnumerable<Lazy<IFileWriter, IFileWriterMetadata>> FileWriters = null;

        public bool Execute(object arg)
        {
            return SaveAsCommand.Execute(Workspace, FileWriters, Workspace.Drawing.Settings.FileName);
        }

        public string DisplayName
        {
            get { return "SAVE"; }
        }
    }
}
