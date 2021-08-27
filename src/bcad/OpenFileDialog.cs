using System.Linq;

namespace bcad.photino
{
    internal static class OpenFileDialog
    {
        public static string OpenFile()
        {
            return OpenFilePlatformSpecific();
        }

        public static (string name, string extension)[] SupportedFileExtensions = new (string, string)[]
        {
            ("DXF File", ".dxf"),
            ("IGES File", ".iges"),
            ("IGS File", ".igs"),
        };

        private static string OpenFilePlatformSpecific()
        {
#if WINDOWS
            var combinedExtensions = string.Join(";", SupportedFileExtensions.Select(ext => $"*{ext.extension}"));
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = $"All CAD files ({combinedExtensions})|{combinedExtensions}|{string.Join("|", SupportedFileExtensions.Select(ext => $"{ext.name}|*{ext.extension}"))}";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    return filePath;
                }
            }
#else
            using (var fcd = new Gtk.FileChooserDialog("Open File", null, Gtk.FileChooserAction.Open))
            {
                fcd.Filter = new Gtk.FileFilter();
                foreach (var (_name, extension) in SupportedFileExtensions)
                {
                    fcd.Filter.AddPattern($"*{extension}");
                }

                fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
                fcd.AddButton(Gtk.Stock.Open, Gtk.ResponseType.Ok);
                fcd.DefaultResponse = Gtk.ResponseType.Ok;
                fcd.SelectMultiple = false;
                Gtk.ResponseType response = (Gtk.ResponseType)fcd.Run();
                if (response == Gtk.ResponseType.Ok)
                {
                    return fcd.Filename;
                }
            }
#endif

            return null;
        }
    }
}
