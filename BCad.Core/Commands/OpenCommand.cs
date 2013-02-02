using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BCad.FileHandlers;
using BCad.Helpers;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Open", "OPEN", ModifierKeys.Control, Key.O, "open", "o")]
    public class OpenCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IUndoRedoService UndoRedoService = null;

        [ImportMany]
        private IEnumerable<Lazy<IFileReader, IFileReaderMetadata>> FileReaders = null;

        private IFileReader ReaderFromExtension(string extension)
        {
            var reader = FileReaders.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension));
            if (reader == null)
                return null;
            return reader.Value;
        }

        public async Task<bool> Execute(object arg)
        {
            if (await Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string filename = null;
            if (arg is string)
                filename = (string)arg;
            if (filename == null)
            {
                filename = await UIHelper.GetFilenameFromUserForOpen(FileReaders.Select(f => new FileSpecification(f.Metadata.DisplayName, f.Metadata.FileExtensions)));
                if (filename == null)
                    return false;
            }

            var extension = Path.GetExtension(filename);
            var reader = ReaderFromExtension(extension);
            if (reader == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            using (var file = new FileStream(filename, FileMode.Open))
            {
                Drawing drawing;
                ViewPort activeViewPort;
                reader.ReadFile(filename, file, out drawing, out activeViewPort);
                if (drawing == null)
                    throw new InvalidOperationException("A drawing must be returned.");
                if (activeViewPort == null)
                    throw new InvalidOperationException("An active viewport must be returned.");
                Workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);
                UndoRedoService.ClearHistory();
            }

            return true;
        }
    }
}
