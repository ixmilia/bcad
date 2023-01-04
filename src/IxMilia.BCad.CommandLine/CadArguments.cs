using System.CommandLine.Parsing;
using System.IO;

namespace IxMilia.BCad.CommandLine
{
    public class CadArguments
    {
        public bool ShowUI { get; }
        public FileInfo DrawingFile { get; }
        public FileInfo BatchFile { get; }

        public CadArguments(bool showUI, FileInfo drawingFile, FileInfo batchFile)
        {
            ShowUI = showUI;
            DrawingFile = drawingFile;
            BatchFile = batchFile;
        }
    }
}
