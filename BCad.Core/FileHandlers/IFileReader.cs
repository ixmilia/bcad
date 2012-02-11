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
        Document ReadFile(Stream stream);
    }
}
