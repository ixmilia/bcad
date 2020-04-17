using System;
using IxMilia.BCad.Commands;

namespace IxMilia.BCad.EventArguments
{
    public abstract class CadCommandEventArgs : EventArgs
    {
        public ICadCommand Command { get; protected set; }

        public CadCommandEventArgs(ICadCommand command)
        {
            Command = command;
        }
    }

    public class CadCommandExecutingEventArgs : CadCommandEventArgs
    {
        public CadCommandExecutingEventArgs(ICadCommand command)
            : base(command)
        {
        }
    }

    public class CadCommandExecutedEventArgs : CadCommandEventArgs
    {
        public CadCommandExecutedEventArgs(ICadCommand command)
            : base(command)
        {
        }
    }
}
