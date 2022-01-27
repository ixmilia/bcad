namespace bcad
{
    internal partial class FileDialogs
    {
        public static (string name, string extension)[] SupportedFileExtensions = new (string, string)[]
        {
            ("DXF File", ".dxf"),
            ("IGES File", ".iges"),
            ("IGS File", ".igs"),
        };
    }
}
