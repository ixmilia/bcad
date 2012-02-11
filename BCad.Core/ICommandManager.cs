using System;
using BCad.Commands;

namespace BCad
{
    public interface ICommandManager
    {
        System.Collections.Generic.IEnumerable<ICommand> Commands { get; set; }
        ICommand GetCommand(string command);
        bool ExecuteCommand(string command, params object[] options);
        void Initialize();
    }
}
