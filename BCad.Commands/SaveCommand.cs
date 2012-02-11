using System.ComponentModel.Composition;

namespace BCad.Commands
{
    [ExportCommand("File.Save", "save", "s")]
    internal class SaveCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public ICommandManager CommandManager { get; set; }

        public bool Execute(params object[] param)
        {
            return CommandManager.ExecuteCommand("File.SaveAs", Workspace.Document.FileName);
        }

        public string DisplayName
        {
            get { return "SAVE"; }
        }
    }
}
