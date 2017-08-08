// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.BCad.EventArguments
{
    public class PromptChangedEventArgs : EventArgs
    {
        public string Prompt { get; private set; }

        public PromptChangedEventArgs(string prompt)
        {
            Prompt = prompt;
        }
    }
}
