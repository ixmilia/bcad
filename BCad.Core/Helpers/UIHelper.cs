using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BCad.Helpers
{
    public class FileSpecification
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public FileSpecification(string displayName, IEnumerable<string> fileExtensions)
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }

    public static class UIHelper
    {
        public static string GetFilenameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications)
        {
            var filter = string.Join("|",
                from fs in fileSpecifications
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

        public static Task<string> GetFilenameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications)
        {
            var filter = string.Join("|",
                    from r in fileSpecifications
                    let exts = string.Join(";", r.FileExtensions.Select(x => "*" + x))
                    select string.Format("{0}|{1}", r.DisplayName, exts));

            var all = string.Format("{0}|{1}",
                "All supported types",
                string.Join(";", fileSpecifications.SelectMany(f => f.FileExtensions).Select(x => "*" + x)));

            filter = string.Join("|", all, filter);

            var dialog = new OpenFileDialog();
            dialog.DefaultExt = fileSpecifications.First().FileExtensions.First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
                return Task.FromResult<string>(null);
            return Task.FromResult<string>(dialog.FileName);
        }
    }
}
