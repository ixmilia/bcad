using IxMilia.BCad.FileHandlers;
using IxMilia.Dwg;

namespace IxMilia.BCad.Extensions
{
    public static class DwgExtensions
    {
        public static DwgFileVersion ToFileVersion(this DwgVersionId version)
        {
            switch (version)
            {
                case DwgVersionId.R13:
                    return DwgFileVersion.R13;
                case DwgVersionId.R14:
                    return DwgFileVersion.R14;
                default:
                    return DwgFileVersion.R14;
            }
        }

        public static DwgVersionId ToDwgVersion(this DwgFileVersion version)
        {
            switch (version)
            {
                case DwgFileVersion.R13:
                    return DwgVersionId.R13;
                case DwgFileVersion.R14:
                    return DwgVersionId.R14;
                default:
                    return DwgVersionId.R14;
            }
        }
    }
}
