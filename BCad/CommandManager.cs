using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BCad.Commands;

namespace BCad
{
    [Export(typeof(ICommandManager))]
    internal class CommandManager : ICommandManager
    {
        private Dictionary<string, ICommand> _commandMap = null;

        [ImportMany]
        public IEnumerable<ICommand> Commands { get; set; }

        public void Initialize()
        {
            if (_commandMap != null)
                return;
            _commandMap = new Dictionary<string, ICommand>();

            // find attributes
            var commandNames = from command in Commands
                               let type = command.GetType()
                               let att = type.GetCustomAttributes(false).OfType<ExportCommandAttribute>().FirstOrDefault()
                               where att != null && att.CommandAliases.Count() > 0
                               select new
                               {
                                   Command = command,
                                   Name = att.Name.ToUpperInvariant(),
                                   Aliases = att.CommandAliases.Select(a => a.ToUpperInvariant())
                               };

            // load aliases
            foreach (var command in commandNames)
            {
                Debug.Assert(!_commandMap.ContainsKey(command.Name));
                _commandMap[command.Name] = command.Command;
                foreach (var alias in command.Aliases)
                {
                    Debug.Assert(!_commandMap.ContainsKey(alias), "Duplicate command alias");
                    _commandMap[alias] = command.Command;
                }
            }
        }

        public ICommand GetCommand(string command)
        {
            if (_commandMap == null)
                Initialize();
            command = command.ToUpperInvariant();
            return _commandMap.ContainsKey(command) ? _commandMap[command] : null;
        }

        public bool ExecuteCommand(string command, params object[] options)
        {
            var com = GetCommand(command);
            if (com == null)
                throw new NullReferenceException("Unable to find command " + command);
            return com.Execute(options);
        }
    }
}
