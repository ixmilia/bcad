using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.FileHandlers;
using Microsoft.Win32;

namespace BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class FileSystemService : IFileSystemService
    {
        [ImportMany]
        public IEnumerable<Lazy<IFileHandler, FileHandlerMetadata>> FileHandlers { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public Task<string> GetFileNameFromUserForSave()
        {
            var x = FileHandlers.Where(fw => fw.Metadata.CanWrite).Select(fw => new FileSpecification(fw.Metadata.DisplayName, fw.Metadata.FileExtensions));
            return GetFileNameFromUserForWrite(x);
        }

        public Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications)
        {
            var filter = string.Join("|",
                from fs in fileSpecifications.OrderBy(f => f.DisplayName)
                let exts = string.Join(";", fs.FileExtensions.Select(x => "*" + x))
                select string.Format("{0}|{1}", fs.DisplayName, exts));

            var dialog = new SaveFileDialog();
            dialog.DefaultExt = fileSpecifications.First().FileExtensions.First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
                return Task.FromResult<string>(null);

            return Task.FromResult(dialog.FileName);
        }

        public Task<string> GetFileNameFromUserForOpen()
        {
            var fileSpecifications = FileHandlers.Where(fr => fr.Metadata.CanRead).Select(fr => new FileSpecification(fr.Metadata.DisplayName, fr.Metadata.FileExtensions));
            var filter = string.Join("|",
                    from r in fileSpecifications.OrderBy(f => f.DisplayName)
                    let exts = string.Join(";", r.FileExtensions.Select(x => "*" + x))
                    select string.Format("{0}|{1}", r.DisplayName, exts));

            var all = string.Format("{0}|{1}",
                "All supported types",
                string.Join(";", fileSpecifications.SelectMany(f => f.FileExtensions).Select(x => "*" + x).OrderBy(x => x)));

            filter = string.Join("|", all, filter);

            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // TODO: this is just for debugging
            dialog.DefaultExt = fileSpecifications.First().FileExtensions.First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
                return Task.FromResult<string>(null);
            return Task.FromResult(dialog.FileName);
        }

        public Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var extension = Path.GetExtension(fileName);
            var writer = WriterFromExtension(extension);
            if (writer == null)
                throw new Exception("Unknown file extension " + extension);

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                writer.WriteDrawing(fileName, fileStream, drawing, viewPort);
            }

            return Task.FromResult(true);
        }

        public Task<bool> TryReadDrawing(string fileName, out Drawing drawing, out ViewPort viewPort)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            drawing = default(Drawing);
            viewPort = default(ViewPort);

            var extension = Path.GetExtension(fileName);
            var reader = ReaderFromExtension(extension);
            if (reader == null)
                throw new Exception("Unknown file extension " + extension);

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                reader.ReadDrawing(fileName, fileStream, out drawing, out viewPort);
                if (viewPort == null)
                {
                    viewPort = drawing.ShowAllViewPort(
                        Vector.ZAxis,
                        Vector.YAxis,
                        Workspace.ViewControl.DisplayWidth,
                        Workspace.ViewControl.DisplayHeight);
                }
            }

            return Task.FromResult(true);
        }

        private IFileHandler ReaderFromExtension(string extension)
        {
            var reader = FileHandlers.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && r.Metadata.CanRead);
            if (reader == null)
                return null;
            return reader.Value;
        }

        private IFileHandler WriterFromExtension(string extension)
        {
            var writer = FileHandlers.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && r.Metadata.CanWrite);
            if (writer == null)
                return null;
            return writer.Value;
        }
    }
}
