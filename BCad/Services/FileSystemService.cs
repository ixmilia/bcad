using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using BCad.FileHandlers;
using Microsoft.Win32;
using System.Reflection;
using BCad.DrawingFiles;

namespace BCad.Services
{
    [Export(typeof(IFileSystemService))]
    internal class FileSystemService : IFileSystemService
    {
        [ImportMany]
        private IEnumerable<Lazy<IFileHandler, IFileHandlerMetadata>> FileHandlers = null;

        public string GetFileNameFromUserForSave()
        {
            var x = FileHandlers.Where(fw => fw.Metadata.CanWrite).Select(fw => new FileSpecification(fw.Metadata.DisplayName, fw.Metadata.FileExtensions));
            return GetFileNameFromUserForWrite(x);
        }

        public string GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications)
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
                return null;

            return dialog.FileName;
        }

        public string GetFileNameFromUserForOpen()
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
                return null;
            return dialog.FileName;
        }

        public bool TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var extension = Path.GetExtension(fileName);
            var writer = WriterFromExtension(extension);
            if (writer == null)
                throw new Exception("Unknown file extension " + extension);

            var converter = writer.GetConverter();
            IDrawingFile drawingFile;
            converter.ConvertFromDrawing(fileName, drawing, viewPort, out drawingFile);

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                drawingFile.Save(fileStream);
            }

            return true;
        }

        public bool TryReadDrawing(string fileName, out Drawing drawing, out ViewPort viewPort)
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
                var drawingFile = reader.Load(fileStream);
                var converter = reader.GetConverter();
                converter.ConvertToDrawing(fileName, drawingFile, out drawing, out viewPort);
            }

            return true;
        }

        private IFileHandler ReaderFromExtension(string extension)
        {
            var reader = FileHandlers.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension) && r.Metadata.CanRead);
            if (reader == null)
                return null;
            return reader.Value;
        }

        private IFileHandler WriterFromExtension(string extension)
        {
            var writer = FileHandlers.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension) && r.Metadata.CanWrite);
            if (writer == null)
                return null;
            return writer.Value;
        }
    }
}
