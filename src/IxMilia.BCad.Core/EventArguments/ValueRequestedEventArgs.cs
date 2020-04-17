using System;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.EventArguments
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
