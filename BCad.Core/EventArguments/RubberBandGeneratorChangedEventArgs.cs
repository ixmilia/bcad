using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
