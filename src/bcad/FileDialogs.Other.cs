namespace bcad
{
    internal partial class FileDialogs
    {
        public static void Init()
        {
            Gtk.Application.Init();
        }

        public static string OpenFile()
        {
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

            return null;
        }

        public static string SaveFile(string extensionHint)
        {
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

            return null;
        }
    }
}
