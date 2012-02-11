using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class DocumentChangingEventArgs : EventArgs
    {
        public Document OldDocument { get; private set; }
        public Document NewDocument { get; private set; }
        public bool Cancel { get; set; }

        public DocumentChangingEventArgs(Document oldDocument, Document newDocument)
        {
            Cancel = false;
            OldDocument = oldDocument;
            NewDocument = newDocument;
        }
    }
}
