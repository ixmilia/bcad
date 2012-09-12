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
    public class SaveAsCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [ImportMany]
        private IEnumerable<IFileWriter> FileWriters = null;

        public bool Execute(object arg)
        {
            string fileName = (arg is string && !string.IsNullOrEmpty((string)arg))
                ? (string)arg
                : null;
            return Execute(Workspace, FileWriters, fileName);
        }

        public static bool Execute(IWorkspace workspace, IEnumerable<IFileWriter> fileWriters, string fileName)
        {
            if (fileName == null)
            {
                fileName = GetFilenameFromUser(fileWriters);
                if (fileName == null)
                    return false;
            }

            Debug.Assert(fileName != null, "Filename should not be null");

            var extension = Path.GetExtension(fileName);
            var writer = WriterFromExtension(fileWriters, extension);
            if (writer == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            using (var file = new FileStream(fileName, FileMode.Create))
            {
                writer.WriteFile(workspace, file);
                var newSettings = workspace.Drawing.Settings.Update(fileName: fileName, isDirty: false);
                workspace.Drawing = workspace.Drawing.Update(settings: newSettings);
            }
            
            return true;
        }

        public string DisplayName
        {
            get { return "SAVEAS"; }
        }

        private static IFileWriter WriterFromExtension(IEnumerable<IFileWriter> fileWriters, string extension)
        {
            return fileWriters.FirstOrDefault(r => r.Extensions().Contains(extension));
        }

        private static string GetFilenameFromUser(IEnumerable<IFileWriter> fileWriters)
        {
            var filter = string.Join("|",
                from w in fileWriters
                let exts = string.Join(";", w.Extensions().Select(x => "*" + x))
                select string.Format("{0}|{1}", w.DisplayName(), exts));

            var dialog = new SaveFileDialog();
            dialog.DefaultExt = fileWriters.First().Extensions().First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
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
