using System.IO;

namespace IxMilia.BCad.CommandLine
{
    public class CadArguments
    {
        public bool ShowUI { get; }
        public FileInfo DrawingFile { get; }
        public FileInfo BatchFile { get; }
        public FileInfo ErrorLog { get; }

        public CadArguments(bool showUI, FileInfo drawingFile, FileInfo batchFile, FileInfo errorLog)
        {
            ShowUI = showUI;
            DrawingFile = drawingFile;
            BatchFile = batchFile;
            ErrorLog = errorLog;
        }
    }
}
