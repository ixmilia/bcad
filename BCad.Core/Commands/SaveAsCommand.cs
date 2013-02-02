using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCad.FileHandlers;
using BCad.Helpers;

namespace BCad.Commands
{
    [ExportCommand("File.SaveAs", "SAVEAS", "saveas", "sa")]
    public class SaveAsCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [ImportMany]
        private IEnumerable<Lazy<IFileWriter, IFileWriterMetadata>> FileWriters = null;

        public Task<bool> Execute(object arg)
        {
            string fileName = (arg is string && !string.IsNullOrEmpty((string)arg))
                ? (string)arg
                : null;
            return Execute(Workspace, FileWriters, fileName);
        }

        public static Task<bool> Execute(IWorkspace workspace, IEnumerable<Lazy<IFileWriter, IFileWriterMetadata>> fileWriters, string fileName)
        {
            if (fileName == null)
            {
                fileName = UIHelper.GetFilenameFromUserForSave(fileWriters.Select(fw => new FileSpecification(fw.Metadata.DisplayName, fw.Metadata.FileExtensions)));
                if (fileName == null)
                    return Task.FromResult<bool>(false);
            }

            Debug.Assert(fileName != null, "Filename should not be null");

            var extension = Path.GetExtension(fileName);
            var writer = WriterFromExtension(fileWriters, extension);
            if (writer == null) // invalid file selected
                throw new Exception("Unknown file extension " + extension);

            using (var file = new FileStream(fileName, FileMode.Create))
            {
                writer.WriteFile(workspace, file);
                if (workspace.Drawing.Settings.FileName != fileName)
                {
                    var newSettings = workspace.Drawing.Settings.Update(fileName: fileName);
                    workspace.Update(drawing: workspace.Drawing.Update(settings: newSettings), isDirty: false);
                }
                else
                {
                    workspace.Update(isDirty: false);
                }
            }
            
            return Task.FromResult<bool>(true);
        }

        private static IFileWriter WriterFromExtension(IEnumerable<Lazy<IFileWriter, IFileWriterMetadata>> fileWriters, string extension)
        {
            var writer = fileWriters.FirstOrDefault(r => r.Metadata.FileExtensions.Contains(extension));
            if (writer == null)
                return null;
            return writer.Value;
        }
    }
}
