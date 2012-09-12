using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BCad.Services
{
    public interface IFileSystemService
    {
        void SaveDrawing(Stream output);
        Drawing OpenDrawing(Stream input);
    }
}
