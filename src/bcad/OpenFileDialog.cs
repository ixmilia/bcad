using System.Linq;

namespace bcad
{
    internal static class OpenFileDialog
    {
        public static string OpenFile()
        {
            return OpenFilePlatformSpecific();
        }

        public static string SaveFile(string extensionHint)
        {
            return SaveFilePlatformSpecific(extensionHint);
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
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = BuildWindowsFileFilter(null);
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

        private static string SaveFilePlatformSpecific(string extensionHint)
        {
#if WINDOWS
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = BuildWindowsFileFilter(extensionHint);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    return filePath;
                }
            }
#else
            using (var fcd = new Gtk.FileChooserDialog("Save File", null, Gtk.FileChooserAction.Save))
            {
                fcd.Filter = new Gtk.FileFilter();
                if (string.IsNullOrWhiteSpace(extensionHint))
                {
                    foreach (var (_name, extension) in SupportedFileExtensions)
                    {
                        fcd.Filter.AddPattern($"*{extension}");
                    }
                }
                else
                {
                    if (!extensionHint.StartsWith("."))
                    {
                        extensionHint = "." + extensionHint;
                    }

                    fcd.Filter.AddPattern($"*{extensionHint}");
                }

                fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
                fcd.AddButton(Gtk.Stock.Save, Gtk.ResponseType.Ok);
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

        private static string BuildWindowsFileFilter(string extensionHint)
        {
            if (!string.IsNullOrWhiteSpace(extensionHint))
            {
                if (!extensionHint.StartsWith("."))
                {
                    extensionHint = "." + extensionHint;
                }

                return $"{extensionHint} files|*{extensionHint}";
            }

            var combinedExtensions = string.Join(";", SupportedFileExtensions.Select(ext => $"*{ext.extension}"));
            var filter = $"All CAD files ({combinedExtensions})|{combinedExtensions}|{string.Join("|", SupportedFileExtensions.Select(ext => $"{ext.name}|*{ext.extension}"))}";
            return filter;
        }
    }
}
