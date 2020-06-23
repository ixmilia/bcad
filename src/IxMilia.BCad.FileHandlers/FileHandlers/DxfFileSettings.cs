namespace IxMilia.BCad.FileHandlers
{
    public enum DxfFileVersion
    {
        // not all versions are reflected here
        R12,
        R13,
        R14,
        R2000,
        R2004,
        R2007,
        R2010,
        R2013
    }

    public class DxfFileSettings
    {
        public DxfFileVersion FileVersion { get; set; }
    }
}
