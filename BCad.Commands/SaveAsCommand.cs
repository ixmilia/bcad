using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BCad.FileHandlers;
using Microsoft.Win32;

namespace BCad.Commands
{
    [ExportCommand("File.SaveAs", "saveas", "sa")]
    internal class SaveAsCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [ImportMany]
        public IEnumerable<IFileWriter> FileWriters { get; set; }

        private IFileWriter WriterFromExtension(string extension)
        {
            return FileWriters.FirstOrDefault(r => r.Extensions().Contains(extension));
        }

        public bool Execute(params object[] param)
        {
            string filename;
            if (param.Length > 0 && param[0] is string)
                filename = (string)param[0];
            else
            {
                filename = GetFilenameFromUser();
                if (filename == null)
                    return false;
            }

            Debug.Assert(filename != null, "Filename should not be null");

            var extension = Path.GetExtension(filename);
            var writer = WriterFromExtension(extension);
            if (writer == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);
            var file = new FileStream(filename, FileMode.Create);
            writer.WriteFile(Workspace.Document, file);
            Workspace.Document.FileName = filename;
            Workspace.Document.Dirty = false;
            return true;
        }

        public string DisplayName
        {
            get { return "SAVEAS"; }
        }

        private string GetFilenameFromUser()
        {
            var filter = string.Join("|",
                from w in FileWriters
                let exts = string.Join(";", w.Extensions().Select(x => "*" + x))
                select string.Format("{0}|{1}", w.DisplayName(), exts));

            var dialog = new SaveFileDialog();
            dialog.DefaultExt = FileWriters.First().Extensions().First();
            dialog.Filter = filter;
            if (!(dialog.ShowDialog() == true))
                return null;
            return dialog.FileName;
        }
    }

    internal static class FileWriterExtensions
    {
        public static IEnumerable<string> Extensions(this IFileWriter writer)
        {
            var att = writer.ExportAttribute();
            if (att == null)
                return null;
            return att.FileExtensions;
        }

        public static string DisplayName(this IFileWriter writer)
        {
            var att = writer.ExportAttribute();
            if (att == null)
                return null;
            return att.DisplayName;
        }

        private static ExportFileWriterAttribute ExportAttribute(this IFileWriter writer)
        {
            var type = writer.GetType();
            return type.GetCustomAttributes(false).OfType<ExportFileWriterAttribute>().FirstOrDefault();
        }
    }
}
