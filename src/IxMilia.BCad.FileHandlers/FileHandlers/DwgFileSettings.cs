namespace IxMilia.BCad.FileHandlers
{
    public enum DwgFileVersion
    {
        R13,
        R14,
    }

    public class DwgFileSettings
    {
        public DwgFileVersion FileVersion { get; set; }
    }
}
