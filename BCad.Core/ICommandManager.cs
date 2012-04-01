using System;
using System.Collections.Generic;
using BCad.Commands;

namespace BCad
{
    public interface ICommandManager
    {
        IEnumerable<ICommand> Commands { get; set; }
        ICommand GetCommand(string command);
        bool ExecuteCommand(string command, params object[] options);
        void Initialize();
    }
}
