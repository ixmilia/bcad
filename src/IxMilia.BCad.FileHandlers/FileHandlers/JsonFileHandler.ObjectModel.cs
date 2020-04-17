using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace IxMilia.BCad.FileHandlers
{
    public partial class JsonFileHandler
    {
        private class JsonDrawing
        {
            [JsonProperty("floorplan")]
            public JsonFloorplan Floorplan { get; set; } = new JsonFloorplan();

            // TODO: metadata
        }

        private class JsonFloorplan
        {
            // TODO: bmp - base64-encoded byte array

            [JsonProperty("bnds")]
            public JsonBounds Bounds { get; set; } = new JsonBounds();

            [JsonProperty("lyrs")]
            public List<JsonLayer> Layers { get; set; } = new List<JsonLayer>();

            [JsonProperty("blks")]
            public List<JsonBlock> Blocks { get; set; } = new List<JsonBlock>();
        }

        private class JsonBounds
        {
            [JsonProperty("x")]
            public double X { get; set; } = 0.0;

            [JsonProperty("y")]
            public double Y { get; set; } = 0.0;

            [JsonProperty("w")]
            public double Width { get; set; } = 0.0;

            [JsonProperty("h")]
            public double Height { get; set; } = 0.0;
        }

        private class JsonLayer
        {
            [JsonProperty("n")]
            public string Name { get; set; } = null;

            [JsonProperty("r")]
            public byte R { get; set; } = 0;

            [JsonProperty("g")]
            public byte G { get; set; } = 0;

            [JsonProperty("b")]
            public byte B { get; set; } = 0;
        }

        private class JsonBlock
        {
            [JsonProperty("n")]
            public string Name { get; set; } = null;

            [JsonProperty("h")]
            public string Handle { get; set; } = null;

            [JsonProperty("ents")]
            public List<JsonEntity> Entities { get; set; } = new List<JsonEntity>();
        }

        private abstract class JsonEntity
        {
            [JsonProperty("t")]
            public string Type
            {
                get
                {
                    switch (this)
                    {
                        case JsonArc _:
                            return "A";
                        case JsonLine _:
                            return "L";
                        default:
                            return null;
                    }
                }
                set { }
            }

            [JsonProperty("h")]
            public string Handle { get; set; } = null;

            [JsonProperty("p")]
            public string ParentHandle { get; set; } = null;

            [JsonProperty("l")]
            public string Layer { get; set; } = null;

            [JsonProperty("d")]
            public string Data { get; set; } = null;

            internal abstract void AfterParse();
            internal abstract void BeforeWrite();
        }

        private class JsonArc : JsonEntity
        {
            [JsonIgnore]
            public Point Center { get; set; }

            [JsonIgnore]
            public double Radius { get; set; }

            [JsonIgnore]
            public double StartAngle { get; set; }

            [JsonIgnore]
            public double EndAngle { get; set; }

            internal override void BeforeWrite()
            {
                Data = $"{Center.X},{Center.Y};{StartAngle};{EndAngle};{Radius}";
            }

            internal override void AfterParse()
            {
                var parts = Data.Split(';');
                Center = Point.Parse(parts[0]);
                StartAngle = double.Parse(parts[1]);
                EndAngle = double.Parse(parts[2]);
                Radius = double.Parse(parts[3]);
            }
        }

        private class JsonLine : JsonEntity
        {
            [JsonIgnore]
            public List<Point> Points { get; set; } = new List<Point>();

            internal override void BeforeWrite()
            {
                Data = string.Join("|", Points.Select(p => $"{p.X},{p.Y}"));
            }

            internal override void AfterParse()
            {
                Points.AddRange(Data.Split('|').Select(Point.Parse));
            }
        }
    }
}
