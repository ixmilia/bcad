// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using BCad.Services;

namespace BCad.EventArguments
{
    public class ValueRequestedEventArgs : EventArgs
    {
        public InputType InputType { get; private set; }

        public ValueRequestedEventArgs(InputType inputType)
        {
            this.InputType = inputType;
        }
    }
}
