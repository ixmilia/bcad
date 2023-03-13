using System;
using Xunit;

namespace IxMilia.BCad.CommandLine.Test
{
    public class CommandLineParserTests
    {
        [Theory]
        [InlineData(null, true, null, null, null)]
        [InlineData("/e error-log.txt", true, null, null, "error-log.txt")]
        [InlineData("my-drawing.dxf", true, "my-drawing.dxf", null, null)]
        [InlineData("/b my-script.scr", true, null, "my-script.scr", null)]
        [InlineData("my-drawing.dxf /b my-script.scr", true, "my-drawing.dxf", "my-script.scr", null)]
        [InlineData("my-drawing.dxf /b my-script.scr /e error-log.txt", true, "my-drawing.dxf", "my-script.scr", "error-log.txt")]
        [InlineData("coreconsole", false, null, null, null)]
        [InlineData("coreconsole /i my-drawing.dxf", false, "my-drawing.dxf", null, null)]
        [InlineData("coreconsole /s my-script.scr", false, null, "my-script.scr", null)]
        [InlineData("coreconsole /i my-drawing.dxf /s my-script.scr", false, "my-drawing.dxf", "my-script.scr", null)]
        [InlineData("coreconsole /i my-drawing.dxf /s my-script.scr /e error-log.txt", false, "my-drawing.dxf", "my-script.scr", "error-log.txt")]
        public void ParseCadCommandLineArguments(string rawCommandLine, bool showUi, string drawingFile, string batchFile, string errorLog)
        {
            var args = rawCommandLine?.Split(" ") ?? Array.Empty<string>();
            var cadArguments = CommandLineParser.Parse(args);
            Assert.Equal(showUi, cadArguments.ShowUI);
            Assert.Equal(drawingFile, cadArguments.DrawingFile?.Name);
            Assert.Equal(batchFile, cadArguments.BatchFile?.Name);
            Assert.Equal(errorLog, cadArguments.ErrorLog?.Name);
        }
    }
}
