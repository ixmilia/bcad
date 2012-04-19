using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad;

namespace BCad.FileHandlers
{
    public interface IFileReader
    {
        void ReadFile(string fileName, Stream stream, out Document document, out Layer currentLayer);
    }
}
