using System;
using Xunit;

namespace IxMilia.BCad.CommandLine.Test
{
    public class CommandLineParserTests
    {
        [Theory]
        [InlineData(null, true, null, null)]
        [InlineData("my-drawing.dxf", true, "my-drawing.dxf", null)]
        [InlineData("/b my-script.scr", true, null, "my-script.scr")]
        [InlineData("my-drawing.dxf /b my-script.scr", true, "my-drawing.dxf", "my-script.scr")]
        [InlineData("coreconsole", false, null, null)]
        [InlineData("coreconsole /i my-drawing.dxf", false, "my-drawing.dxf", null)]
        [InlineData("coreconsole /s my-script.scr", false, null, "my-script.scr")]
        [InlineData("coreconsole /i my-drawing.dxf /s my-script.scr", false, "my-drawing.dxf", "my-script.scr")]
        public void ParseCadCommandLineArguments(string rawCommandLine, bool showUi, string drawingFile, string batchFile)
        {
            var args = rawCommandLine?.Split(" ") ?? Array.Empty<string>();
            var cadArguments = CommandLineParser.Parse(args);
            Assert.Equal(showUi, cadArguments.ShowUI);
            Assert.Equal(drawingFile, cadArguments.DrawingFile?.Name);
            Assert.Equal(batchFile, cadArguments.BatchFile?.Name);
        }
    }
}
