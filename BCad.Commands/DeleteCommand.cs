using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BCad.Objects;

namespace BCad.Commands
{
    [ExportCommand("Object.Delete", ModifierKeys.None, Key.Delete, "delete", "d", "del")]
    internal class DeleteCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(params object[] parameters)
        {
            var directive = new UserDirective("Select object");
            var objects = new List<IObject>();
            var input = InputService.GetObject(directive);
            while (input.HasValue)
            {
                InputService.WriteLine("Found object {0}", input.Value);
                objects.Add(input.Value);
                input = InputService.GetObject(directive);
            }

            var doc = Workspace.Document;
            foreach (var obj in objects)
            {
                doc = doc.Remove(obj);
            }

            Workspace.Document = doc;
            return true;
        }

        public string DisplayName
        {
            get { return "DELETE"; }
        }
    }
}
