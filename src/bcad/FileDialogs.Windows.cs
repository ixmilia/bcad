using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace bcad
{
    internal partial class FileDialogs
    {
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetSaveFileName(ref OpenFileName lpofn);

        public static void Init()
        {
        }

        public static string OpenFile()
        {
            var ofn = GetFileSpecification("Open Drawing...", null);
            if (GetOpenFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }

            return null;
        }

        public static string SaveFile(string extensionHint)
        {
            var ofn = GetFileSpecification("Save Drawing...", extensionHint);
            if (GetSaveFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }

            return null;
        }

        private static OpenFileName GetFileSpecification(string title, string extensionHint)
        {
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = BuildWindowsFileFilter(extensionHint);
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = title;
            return ofn;
        }

        private const char FileFilterPatternSeparator = ';';
        private const char FileFilterPairSeparator = '\0';
        private const string FilterTerminator = "\0\0";

        private static string BuildWindowsFileFilter(string extensionHint)
        {
            if (!string.IsNullOrWhiteSpace(extensionHint))
            {
                if (!extensionHint.StartsWith("."))
                {
                    extensionHint = "." + extensionHint;
                }

                return $"{extensionHint} files{FileFilterPairSeparator}*{extensionHint}{FilterTerminator}";
            }

            var combinedExtensions = string.Join(FileFilterPatternSeparator, SupportedFileExtensions.Select(ext => $"*{ext.extension}"));
            var filter = $"All CAD files ({combinedExtensions}){FileFilterPairSeparator}{combinedExtensions}{FileFilterPairSeparator}{string.Join(FileFilterPairSeparator, SupportedFileExtensions.Select(ext => $"{ext.name} ({ext.extension}){FileFilterPairSeparator}*{ext.extension}"))}{FilterTerminator}";
            return filter;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }
}
