using System;

namespace IxMilia.BCad.EventArguments
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
