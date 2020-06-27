namespace IxMilia.BCad.Services
{
    public class FileSettings
    {
        public string Extension { get; }
        public object Settings { get; }

        public FileSettings(string extension, object settings)
        {
            Extension = extension;
            Settings = settings;
        }
    }
}
