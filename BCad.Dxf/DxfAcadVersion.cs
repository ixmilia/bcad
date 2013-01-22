using System;

namespace BCad.Dxf
{
    public enum DxfAcadVersion
    {
        R10,
        R11,
        R12,
        R13,
        R14
    }

    public static class DxfAcadVersionStrings
    {
        public const string R10 = "AC1006";
        public const string R11 = "AC1009";
        public const string R12 = "AC1009";
        public const string R13 = "AC1012";
        public const string R14 = "AC1014";

        public static string VersionToString(DxfAcadVersion version)
        {
            switch (version)
            {
                case DxfAcadVersion.R10:
                    return R10;
                case DxfAcadVersion.R11:
                    return R11;
                case DxfAcadVersion.R12:
                    return R12;
                case DxfAcadVersion.R13:
                    return R13;
                case DxfAcadVersion.R14:
                    return R14;
                default:
                    throw new NotSupportedException();
            }
        }

        public static DxfAcadVersion StringToVersion(string str)
        {
            switch (str)
            {
                case R10:
                    return DxfAcadVersion.R10;
                case R11:
                // case R12:
                    return DxfAcadVersion.R12;
                case R13:
                    return DxfAcadVersion.R13;
                case R14:
                    return DxfAcadVersion.R14;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
