// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
