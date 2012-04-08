using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.Commands
{
    [ExportCommand("Object.Delete", "delete", "d", "del")]
    internal class DeleteCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        public bool Execute(params object[] parameters)
        {
            var input = InputService.GetObject(new UserDirective("Select objects"));
            if (input.Cancel) return false;

            var objects = new List<IObject>();
            objects.Add(input.Value);
            InputService.WriteLine("Found object: {0}", input.Value.ToString());

            return true;
        }

        public string DisplayName
        {
            get { return "DELETE"; }
        }
    }
}
