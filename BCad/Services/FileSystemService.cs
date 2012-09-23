using System;
using System.ComponentModel.Composition;
using System.IO;

namespace BCad.Services
{
    [Export(typeof(IFileSystemService))]
    internal class FileSystemService : IFileSystemService
    {
        [Import]
        private IWorkspace Workspace { get; set; }

        public void WriteDrawing(Drawing drawing, Stream output)
        {
            throw new NotImplementedException();
        }

        public Drawing ReadDrawing(Stream input)
        {
            throw new NotImplementedException();
        }
    }
}
