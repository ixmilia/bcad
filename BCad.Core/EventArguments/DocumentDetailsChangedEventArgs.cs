using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class DocumentDetailsChangedEventArgs : EventArgs
    {
        public Document Document { get; private set; }

        public DocumentDetailsChangedEventArgs(Document document)
        {
            Document = document;
        }
    }
}
