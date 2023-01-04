using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using IxMilia.BCad.Services;

namespace bcad
{
    internal partial class FileDialogs
    {
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetSaveFileName(ref OpenFileName lpofn);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public static void Init()
        {
        }

        public static void GetConsole()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }

        public static string OpenFile(IEnumerable<FileSpecification> fileSpecifications)
        {
            var ofn = GetFileSpecification("Open File", fileSpecifications);
            if (GetOpenFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }

            return null;
        }

        public static string SaveFile(IEnumerable<FileSpecification> fileSpecifications)
        {
            var ofn = GetFileSpecification("Save File", fileSpecifications);
            if (GetSaveFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }

            return null;
        }

        private static OpenFileName GetFileSpecification(string title, IEnumerable<FileSpecification> fileSpecifications)
        {
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = BuildWindowsFileFilter(fileSpecifications);
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = title;
            ofn.Flags = OFN_OVERWRITEPROMPT;
            var extensionHint = fileSpecifications.First().FileExtensions.First();
            if (extensionHint.StartsWith("."))
            {
                extensionHint = extensionHint.Substring(1);
            }

            ofn.lpstrDefExt = extensionHint;

            return ofn;
        }

        private const char FileFilterPatternSeparator = ';';
        private const char FileFilterPairSeparator = '\0';
        private const string FilterTerminator = "\0\0";
        private const int OFN_OVERWRITEPROMPT = 0x00000002;

        private static string BuildWindowsFileFilter(IEnumerable<FileSpecification> fileSpecifications)
        {
            var filter = $"{string.Join(FileFilterPairSeparator, fileSpecifications.Select(spec => $"{spec.DisplayName} ({string.Join(FileFilterPatternSeparator, spec.FileExtensions)}){FileFilterPairSeparator}{string.Join(FileFilterPatternSeparator, spec.FileExtensions.Select(ext => $"*{ext}"))}"))}{FilterTerminator}";
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
