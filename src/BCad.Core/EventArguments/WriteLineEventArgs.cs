// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.EventArguments
{
    public class WriteLineEventArgs : EventArgs
    {
        public string Line { get; private set; }

        public WriteLineEventArgs(string line)
        {
            Line = line;
        }
    }
}
