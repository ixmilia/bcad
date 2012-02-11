using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Commands;

namespace BCad.EventArguments
{
    public abstract class CommandEventArgs : EventArgs
    {
        public ICommand Command { get; protected set; }

        public CommandEventArgs(ICommand command)
        {
            Command = command;
        }
    }

    public class CommandExecutingEventArgs : CommandEventArgs
    {
        public CommandExecutingEventArgs(ICommand command)
            : base(command)
        {
        }
    }

    public class CommandExecutedEventArgs : CommandEventArgs
    {
        public CommandExecutedEventArgs(ICommand command)
            : base(command)
        {
        }
    }
}
