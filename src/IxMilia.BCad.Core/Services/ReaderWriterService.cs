using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.FileHandlers;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Services
{
    internal class ReaderWriterService : IReaderWriterService
    {
        private IWorkspace _workspace;
        private Dictionary<Drawing, object> _drawingSettingsCache = new Dictionary<Drawing, object>();
        private List<FileHandlerData> _fileHandlers = new List<FileHandlerData>();

        public ReaderWriterService(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void RegisterFileHandler(IFileHandler fileHandler, bool canRead, bool canWrite, params string[] fileExtensions)
        {
            _fileHandlers.Add(new FileHandlerData(fileHandler, canRead, canWrite, fileExtensions));
        }

        public async Task<ReadDrawingResult> ReadDrawing(string fileName, Stream stream, Func<string, Task<byte[]>> contentResolver)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var extension = Path.GetExtension(fileName);
            var reader = ReaderFromExtension(extension);
            if (reader == null)
            {
                throw new Exception("Unknown file extension " + extension);
            }

            var result = await reader.ReadDrawing(fileName, stream, contentResolver);

            if (result.Success && result.ViewPort == null)
            {
                result = ReadDrawingResult.Succeeded(
                    result.Drawing,
                    result.Drawing.ShowAllViewPort(
                        _workspace.ActiveViewPort.Sight,
                        _workspace.ActiveViewPort.Up,
                        _workspace.ViewControl.DisplayWidth,
                        _workspace.ViewControl.DisplayHeight));
            }

            return result;
        }

        public async Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort, Stream stream, bool preserveSettings = true)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var extension = Path.GetExtension(fileName);
            var writer = WriterFromExtension(extension);
            if (writer == null)
            {
                throw new Exception("Unknown file extension " + extension);
            }

            object fileSettings = null;
            if (!preserveSettings)
            {
                fileSettings = writer.GetFileSettingsFromDrawing(drawing);
            }

            if (fileSettings != null)
            {
                var parameter = new FileSettings(extension.ToLower(), fileSettings);
                if (_workspace.DialogService != null)
                {
                    fileSettings = await _workspace.DialogService.ShowDialog("FileSettings", parameter);
                    if (fileSettings is null)
                    {
                        return false;
                    }
                }
            }

            _drawingSettingsCache.TryGetValue(drawing, out var previousDrawingSettings);
            await writer.WriteDrawing(fileName, stream, drawing, viewPort, fileSettings ?? previousDrawingSettings);

            if (fileSettings != null)
            {
                _drawingSettingsCache[drawing] = fileSettings;
            }

            return true;
        }

        private IFileHandler ReaderFromExtension(string extension)
        {
            var reader = _fileHandlers.FirstOrDefault(r => r.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && r.CanRead);
            if (reader == null)
                return null;
            return reader.FileHandler;
        }

        private IFileHandler WriterFromExtension(string extension)
        {
            var writer = _fileHandlers.FirstOrDefault(r => r.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && r.CanWrite);
            if (writer == null)
                return null;
            return writer.FileHandler;
        }

        private class FileHandlerData
        {
            public IFileHandler FileHandler { get; }
            public bool CanRead { get; }
            public bool CanWrite { get; }
            public IReadOnlyList<string> FileExtensions { get; }

            public FileHandlerData(IFileHandler fileHandler, bool canRead, bool canWrite, params string[] fileExtensions)
            {
                FileHandler = fileHandler;
                CanRead = canRead;
                CanWrite = canWrite;
                FileExtensions = fileExtensions.ToList();
            }
        }
    }
}
