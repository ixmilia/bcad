using System.IO;

namespace BCad.Core
{
    public interface IDrawingFile
    {
        void Save(Stream stream);
    }
}
