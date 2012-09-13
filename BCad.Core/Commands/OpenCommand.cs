using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BCad.FileHandlers;
using BCad.Services;
using Microsoft.Win32;

namespace BCad.Commands
{
    [ExportCommand("File.Open", ModifierKeys.Control, Key.O, "open", "o")]
    public class OpenCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        [ImportMany]
        public IEnumerable<IFileReader> FileReaders { get; set; }

        private IFileReader ReaderFromExtension(string extension)
        {
            return FileReaders.FirstOrDefault(r => r.Extensions().Contains(extension));
        }

        public bool Execute(object arg)
        {
            if (Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string filename = null;
            if (arg is string)
                filename = (string)arg;
            if (filename == null)
            {
                var filter = string.Join("|",
                    from r in FileReaders
                    let exts = string.Join(";", r.Extensions().Select(x => "*" + x))
                    select string.Format("{0}|{1}", r.DisplayName(), exts));

                var dialog = new OpenFileDialog();
                dialog.DefaultExt = FileReaders.First().Extensions().First();
                dialog.Filter = filter;
                var result = dialog.ShowDialog();
                if (result != true)
                    return false;
                filename = dialog.FileName;
            }

            var extension = Path.GetExtension(filename);
            var reader = ReaderFromExtension(extension);
            if (reader == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            using (var file = new FileStream(filename, FileMode.Open))
            {
                var drawing = reader.ReadFile(filename, file);
                Workspace.Drawing = drawing;
                UndoRedoService.ClearHistory();
            }

            return true;
        }

        public string DisplayName
        {
            get { return "OPEN"; }
        }
    }

    internal static class FileReaderExtensions
    {
        public static IEnumerable<string> Extensions(this IFileReader reader)
        {
            var att = reader.ExportAttribute();
            if (att == null)
                return null;
            return att.FileExtensions;
        }

        public static string DisplayName(this IFileReader reader)
        {
            var att = reader.ExportAttribute();
            if (att == null)
                return null;
            return att.DisplayName;
        }

        private static ExportFileReaderAttribute ExportAttribute(this IFileReader reader)
        {
            var type = reader.GetType();
            return type.GetCustomAttributes(false).OfType<ExportFileReaderAttribute>().FirstOrDefault();
        }
    }
}
