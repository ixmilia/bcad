using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BCad.FileHandlers
{
    public interface IFileWriter
    {
        void WriteFile(IWorkspace workspace, Stream stream);
    }
}
