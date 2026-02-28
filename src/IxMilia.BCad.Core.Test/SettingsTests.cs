using System;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class SettingsTests : TestBase
    {
        [Fact]
        public void ParseSettingsFile()
        {
            var content = """
                AnglePrecision = 0
                Debug = False
                DrawingPrecision = 8
                DrawingUnits = Metric
                UnitFormat = Decimal

                [Display]
                AngleSnap = True
                BackgroundColor = #FF2F2F2F
                CursorSize = 60
                EntitySelectionRadius = 3
                HotPointColor = #FF0000FF
                HotPointSize = 10
                Ortho = False
                PointDisplaySize = 48
                PointSnap = True
                RenderId = canvas
                SnapAngleDistance = 30
                SnapAngles = 0;90;180;270
                SnapPointColor = #FFFFFF00
                SnapPointDistance = 15
                SnapPointSize = 15
                TextCursorSize = 18
                """;
            var lines = content.Split('\n');
            Workspace.SettingsService.LoadFromLines(lines);

            Assert.Equal(0, Workspace.SettingsService.GetValue<int>("AnglePrecision"));
            Assert.False(Workspace.SettingsService.GetValue<bool>("Debug"));
            Assert.Equal(8, Workspace.SettingsService.GetValue<int>("DrawingPrecision"));
            Assert.Equal(DrawingUnits.Metric, Workspace.SettingsService.GetValue<DrawingUnits>("DrawingUnits"));
            Assert.Equal(UnitFormat.Decimal, Workspace.SettingsService.GetValue<UnitFormat>("UnitFormat"));
            Assert.True(Workspace.SettingsService.GetValue<bool>("Display.AngleSnap"));
            Assert.Equal(CadColor.FromUInt32(0xFF2F2F2F), Workspace.SettingsService.GetValue<CadColor>("Display.BackgroundColor"));
            Assert.Equal(60, Workspace.SettingsService.GetValue<int>("Display.CursorSize"));
            Assert.Equal(3.0, Workspace.SettingsService.GetValue<double>("Display.EntitySelectionRadius"));
            Assert.Equal(CadColor.FromUInt32(0xFF0000FF), Workspace.SettingsService.GetValue<CadColor>("Display.HotPointColor"));
            Assert.Equal(10.0, Workspace.SettingsService.GetValue<double>("Display.HotPointSize"));
            Assert.False(Workspace.SettingsService.GetValue<bool>("Display.Ortho"));
            Assert.Equal(48.0, Workspace.SettingsService.GetValue<double>("Display.PointDisplaySize"));
            Assert.True(Workspace.SettingsService.GetValue<bool>("Display.PointSnap"));
            Assert.Equal("canvas", Workspace.SettingsService.GetValue<string>("Display.RenderId"));
            Assert.Equal(30.0, Workspace.SettingsService.GetValue<double>("Display.SnapAngleDistance"));
            Assert.Equal([0.0, 90.0, 180.0, 270.0], Workspace.SettingsService.GetValue<double[]>("Display.SnapAngles"));
            Assert.Equal(CadColor.FromUInt32(0xFFFFFF00), Workspace.SettingsService.GetValue<CadColor>("Display.SnapPointColor"));
            Assert.Equal(15.0, Workspace.SettingsService.GetValue<double>("Display.SnapPointDistance"));
            Assert.Equal(15.0, Workspace.SettingsService.GetValue<double>("Display.SnapPointSize"));
            Assert.Equal(18, Workspace.SettingsService.GetValue<int>("Display.TextCursorSize"));
        }

        [Fact]
        public void SaveSettingsFile()
        {
            Workspace.SettingsService.SetValue("AnglePrecision", 0);
            Workspace.SettingsService.SetValue("Debug", false);
            Workspace.SettingsService.SetValue("DrawingPrecision", 8);
            Workspace.SettingsService.SetValue("DrawingUnits", DrawingUnits.Metric);
            Workspace.SettingsService.SetValue("UnitFormat", UnitFormat.Decimal);
            Workspace.SettingsService.SetValue("Display.AngleSnap", true);
            Workspace.SettingsService.SetValue("Display.BackgroundColor", CadColor.FromUInt32(0xFF2F2F2F));
            Workspace.SettingsService.SetValue("Display.CursorSize", 60);
            Workspace.SettingsService.SetValue("Display.EntitySelectionRadius", 3.0);
            Workspace.SettingsService.SetValue("Display.HotPointColor", CadColor.FromUInt32(0xFF0000FF));
            Workspace.SettingsService.SetValue("Display.HotPointSize", 10.0);
            Workspace.SettingsService.SetValue("Display.Ortho", false);
            Workspace.SettingsService.SetValue("Display.PointDisplaySize", 48.0);
            Workspace.SettingsService.SetValue("Display.PointSnap", true);
            Workspace.SettingsService.SetValue("Display.RenderId", "canvas");
            Workspace.SettingsService.SetValue("Display.SnapAngleDistance", 30.0);
            Workspace.SettingsService.SetValue("Display.SnapAngles", new double[] { 0.0, 90.0, 180.0, 270.0 });
            Workspace.SettingsService.SetValue("Display.SnapPointColor", CadColor.FromUInt32(0xFFFFFF00));
            Workspace.SettingsService.SetValue("Display.SnapPointDistance", 15.0);
            Workspace.SettingsService.SetValue("Display.SnapPointSize", 15.0);
            Workspace.SettingsService.SetValue("Display.TextCursorSize", 18);
            var actual = Workspace.SettingsService.WriteWithLines([]).Replace("\r", "");
            var expected = """
                AnglePrecision = 0
                Debug = False
                DrawingPrecision = 8
                DrawingUnits = Metric
                UnitFormat = Decimal
                
                [Display]
                AngleSnap = True
                BackgroundColor = #FF2F2F2F
                CursorSize = 60
                EntitySelectionRadius = 3
                HotPointColor = #FF0000FF
                HotPointSize = 10
                Ortho = False
                PointDisplaySize = 48
                PointSnap = True
                RenderId = canvas
                SnapAngleDistance = 30
                SnapAngles = 0;90;180;270
                SnapPointColor = #FFFFFF00
                SnapPointDistance = 15
                SnapPointSize = 15
                TextCursorSize = 18
                """.Replace("\r", "");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SetValueFromString()
        {
            var initialCursorSize = Workspace.SettingsService.GetValue<int>("Display.CursorSize");
            Assert.NotEqual(4242, initialCursorSize);

            Workspace.SettingsService.SetValueFromString("Display.CursorSize", "4242");

            var finalCursorSize = Workspace.SettingsService.GetValue<int>("Display.CursorSize");
            Assert.Equal(4242, finalCursorSize);
        }

        [Theory]
        [InlineData(typeof(bool), "TRUE", "True")] // boolean - valid
        [InlineData(typeof(bool), "indeed", "False")] // boolean - invalid
        [InlineData(typeof(int), "42", "42")] // int - valid
        [InlineData(typeof(int), "not-an-int", "0")] // int - invalid
        [InlineData(typeof(double), "3.5", "3.5")] // double - valid
        [InlineData(typeof(double), "not-a-double", "0")] // double - invalid
        [InlineData(typeof(string), "a string value", "a string value")] // string - valid
        [InlineData(typeof(CadColor), "#FF2F2F2F", "#FF2F2F2F")] // CadColor - valid
        [InlineData(typeof(CadColor), "a-beautiful-color", "#00000000")] // CadColor - invalid
        [InlineData(typeof(DrawingUnits), "Metric", "Metric")] // DrawingUnits - valid
        [InlineData(typeof(DrawingUnits), "2", "English")] // DrawingUnits - invalid
        [InlineData(typeof(UnitFormat), "Decimal", "Decimal")] // UnitFormat - valid
        [InlineData(typeof(UnitFormat), "not-a-unit-format", "Architectural")] // UnitFormat - invalid
        [InlineData(typeof(double[]), "0;90;180;270", "0;90;180;270")] // double[] - valid
        [InlineData(typeof(double[]), "not-an-array", "0")] // double[] - invalid
        public void SetInvalidValueFromStringSetsReasonableDefault(Type valueType, string originalValue, string expectedString)
        {
            var parsedValue = Workspace.SettingsService.StringToValue(valueType, originalValue);
            var actualString = Workspace.SettingsService.ValueToString(valueType, parsedValue);
            Assert.Equal(expectedString, actualString);
        }
    }
}
