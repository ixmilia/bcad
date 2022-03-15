using System.IO;
using IxMilia.BCad.Plotting;
using Newtonsoft.Json;
using Xunit;

namespace IxMilia.BCad.Rpc.Test
{
    public class MessageSerializationTests
    {
        private static JsonSerializer GetSerializer()
        {
            var serializer = new JsonSerializer();
            Serializer.PrepareSerializer(serializer);
            return serializer;
        }

        private static T Deserialize<T>(string json)
        {
            var serializer = GetSerializer();
            var jsonReader = new JsonTextReader(new StringReader(json));
            var result = serializer.Deserialize<T>(jsonReader);
            return result;
        }

        [Fact]
        public void ClientPlotSettingsDeserialization()
        {
            var json = @"{""PlotType"":""pdf"",""Viewport"":{""TopLeft"":{""X"":4,""Y"":5,""Z"":6},""BottomRight"":{""X"":7,""Y"":8,""Z"":9}},""ScaleA"":""1.0"",""ScaleB"":""2.0"",""ScalingType"":""ToFit"",""ViewPortType"":""Window"",""ColorType"":""Contrast"",""Width"":100,""Height"":200,""PreviewMaxSize"":300}";
            var result = Deserialize<ClientPlotSettings>(json);
            Assert.Equal("pdf", result.PlotType);
            Assert.Equal(new Point(4.0, 5.0, 6.0), result.Viewport.TopLeft.ToPoint());
            Assert.Equal(new Point(7.0, 8.0, 9.0), result.Viewport.BottomRight.ToPoint());
            Assert.Equal("1.0", result.ScaleA);
            Assert.Equal("2.0", result.ScaleB);
            Assert.Equal(PlotScalingType.ToFit, result.ScalingType);
            Assert.Equal(PlotViewPortType.Window, result.ViewPortType);
            Assert.Equal(PlotColorType.Contrast, result.ColorType);
            Assert.Equal(100.0, result.Width);
            Assert.Equal(200.0, result.Height);
            Assert.Equal(300.0, result.PreviewMaxSize);
        }
    }
}
