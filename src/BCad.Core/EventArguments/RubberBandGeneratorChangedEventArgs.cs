﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace BCad.EventArguments
{
    public class RubberBandGeneratorChangedEventArgs : EventArgs
    {
        public RubberBandGenerator Generator { get; private set; }

        public RubberBandGeneratorChangedEventArgs(RubberBandGenerator generator)
        {
            Generator = generator;
        }
    }
}
