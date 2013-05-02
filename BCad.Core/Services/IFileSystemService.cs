using System.Collections.Generic;

namespace BCad.Services
{
    public interface IFileSystemService
    {
        string GetFileNameFromUserForSave();
        string GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications);
        string GetFileNameFromUserForOpen();
        bool TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort);
        bool TryReadDrawing(string fileName, out Drawing drawing, out ViewPort viewPort);
    }
}
