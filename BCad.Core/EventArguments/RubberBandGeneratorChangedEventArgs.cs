using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Services;

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
