using System.IO;

namespace BCad.Services
{
    public interface IFileSystemService
    {
        void WriteDrawing(Drawing drawing, Stream output);
        Drawing ReadDrawing(Stream input);
    }
}
