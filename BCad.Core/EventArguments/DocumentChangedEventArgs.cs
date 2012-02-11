using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class DocumentChangedEventArgs : EventArgs
    {
        public Document Document { get; private set; }

        public DocumentChangedEventArgs(Document document)
        {
            Document = document;
        }
    }
}
