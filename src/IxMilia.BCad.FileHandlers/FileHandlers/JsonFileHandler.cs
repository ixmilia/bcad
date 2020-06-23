using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IxMilia.BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, true, FileExtension)]
    public partial class JsonFileHandler : IFileHandler
    {
        public const string DisplayName = "Json";
        public const string FileExtension = ".json";

        private JsonSerializerSettings Settings = new JsonSerializerSettings() { Converters = { new JsonEntityConverter() } };

        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            return null;
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            ReadOnlyTree<string, Layer> layers = null;
            using (var reader = new StreamReader(fileStream, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                var json = reader.ReadToEnd();
                var jsonDrawing = JsonConvert.DeserializeObject<JsonDrawing>(json, Settings);
                var layerList = new List<Layer>();
                foreach (var jsonLayer in jsonDrawing.Floorplan.Layers)
                {
                    var layer = new Layer(jsonLayer.Name, color: CadColor.FromArgb(255, jsonLayer.R, jsonLayer.G, jsonLayer.B));
                    layerList.Add(layer);
                }

                layers = ReadOnlyTree<string, Layer>.FromEnumerable(layerList, l => l.Name);
                void AddEntityToLayer(string layerName, Entity entity)
                {
                    var layer = layers.GetValue(layerName);
                    layer = layer.Add(entity);
                    layers = layers.Insert(layerName, layer);
                }

                foreach (var block in jsonDrawing.Floorplan.Blocks)
                {
                    switch (block.Name.ToUpperInvariant())
                    {
                        case "*MODEL_SPACE":
                            foreach (var jsonEntity in block.Entities)
                            {
                                switch (jsonEntity)
                                {
                                    case JsonArc jsonArc:
                                        var arc = new Arc(jsonArc.Center, jsonArc.Radius, jsonArc.StartAngle * MathHelper.RadiansToDegrees, jsonArc.EndAngle * MathHelper.RadiansToDegrees, Vector.ZAxis);
                                        AddEntityToLayer(jsonEntity.Layer, arc);
                                        break;
                                    case JsonLine jsonLine:
                                        for (int i = 0; i < jsonLine.Points.Count - 1; i++)
                                        {
                                            AddEntityToLayer(jsonEntity.Layer, new Line(jsonLine.Points[i], jsonLine.Points[i + 1]));
                                        }

                                        if (jsonLine.Points.Count > 2)
                                        {
                                            AddEntityToLayer(jsonEntity.Layer, new Line(jsonLine.Points.Last(), jsonLine.Points.First()));
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        // TODO: handle other blocks?
                    }
                }
            }

            drawing = new Drawing().Update(layers: layers);
            viewPort = null; // auto-generate later

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            var jsonDrawing = new JsonDrawing();
            var modelSpace = new JsonBlock() { Name = "*Model_Space" };
            jsonDrawing.Floorplan.Blocks.Add(modelSpace);
            foreach (var layer in drawing.GetLayers())
            {
                var jsonLayer = new JsonLayer()
                {
                    Name = layer.Name,
                    R = layer.Color?.R ?? 0,
                    G = layer.Color?.G ?? 0,
                    B = layer.Color?.B ?? 0,
                };
                jsonDrawing.Floorplan.Layers.Add(jsonLayer);
                foreach (var entity in layer.GetEntities())
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Arc:
                            var arc = (Arc)entity;
                            var jsonArc = new JsonArc()
                            {
                                Layer = layer.Name,
                                Center = arc.Center,
                                Radius = arc.Radius,
                                StartAngle = arc.StartAngle * MathHelper.DegreesToRadians,
                                EndAngle = arc.EndAngle * MathHelper.DegreesToRadians
                            };
                            modelSpace.Entities.Add(jsonArc);
                            break;
                        case EntityKind.Line:
                            var line = (Line)entity;
                            var jsonLine = new JsonLine() { Layer = layer.Name };
                            jsonLine.Points.Add(line.P1);
                            jsonLine.Points.Add(line.P2);
                            modelSpace.Entities.Add(jsonLine);
                            break;
                    }
                }

                foreach (var jsonEntity in modelSpace.Entities)
                {
                    jsonEntity.BeforeWrite();
                }
            }

            var json = JsonConvert.SerializeObject(jsonDrawing, Settings);
            using (var writer = new StreamWriter(fileStream, encoding: Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
            {
                writer.Write(json);
            }

            return true;
        }

        private class JsonEntityConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(JsonEntity);
            }

            public override bool CanWrite => true;
            public override bool CanRead => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jo = JObject.Load(reader);
                var typeString = (string)jo["t"];
                JsonEntity entity;
                switch (typeString)
                {
                    case "A":
                        entity = jo.ToObject<JsonArc>(serializer);
                        break;
                    case "B":
                        // example:
                        // {
                        //   "t": "B",
                        //   "h": "7D84",
                        //   "p": "1F",
                        //   "l": "S-GRID-IDEN-EXST",
                        //   "c": {
                        //     "t": "L"
                        //   },
                        //   "b": "4216",
                        //   "d": "4557.99999600003,2281.88345914876;96;96;0",
                        //   "atts": [
                        //     {}
                        //   ]
                        // }
                        entity = null;
                        break;
                    case "C":
                        // example:
                        // {
                        //  "t": "C",
                        //  "h": "CDE7",
                        //  "p": "1F",
                        //  "l": "A-FURN-FREE-EXST",
                        //  "c": {
                        //    "t": "L"
                        //  },
                        //  "sents": [
                        //    {
                        //      "t": "P",
                        //      "d": "5129.17346025466,2638.33635654058;5129.15750822654,2638.56059957781;0.00398172573236266"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5129.15750822654,2638.56059957781;5129.11313297802,2638.77062202288;0.00355658289033234"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5129.11313297802,2638.77062202288;5128.98882765085,2639.08644509474;0.00845993397036615"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5128.98882765085,2639.08644509474;5128.7990407962,2639.38302813977;0.00831280783924693"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5128.7990407962,2639.38302813977;5128.51404775427,2639.68132566635;0.0101618318638597"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5128.51404775427,2639.68132566635;5128.12951303121,2639.95077986862;0.0115510842690671"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5128.12951303121,2639.95077986862;5127.48432163509,2640.21199647146;0.0221325367753984"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5127.48432163509,2640.21199647146;5126.70551727018,2640.31293966962;0.0255343910578292"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5126.70551727018,2640.31293966962;5125.92673419451,2640.21183122829;0.0255344078661228"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5125.92673419451,2640.21183122829;5125.28159800999,2639.95047749287;0.0221325749904388"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5125.28159800999,2639.95047749287;5124.89712043488,2639.68094137149;0.0115511099028788"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5124.89712043488,2639.68094137149;5124.61219085276,2639.38258301195;0.0101618546467232"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5124.61219085276,2639.38258301195;5124.42246725259,2639.08595939151;0.00831282405976297"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5124.42246725259,2639.08595939151;5124.2982360552,2638.7701333738;0.00845870127787784"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5124.2982360552,2638.7701333738;5124.25389907874,2638.56007776809;0.00355739311568803"
                        //    },
                        //    {
                        //      "t": "P",
                        //      "d": "5124.25389907874,2638.56007776809;5124.23799502949,2638.33583635003;0.00398154844077303"
                        //    }
                        //  ]
                        // }
                        entity = null;
                        break;
                    case "F":
                        // example:
                        // {
                        //  "t": "F",
                        //  "h": "3AA4",
                        //  "p": "3A9F",
                        //  "l": "0",
                        //  "c": {
                        //    "t": "C",
                        //    "r": "102",
                        //    "g": "204",
                        //    "b": "204"
                        //  },
                        //  "loops": [
                        //    [
                        //      {
                        //        "t": "L",
                        //        "d": "7.75000536403203,5.0000188823324|7.75000536403203,-4.99998107575811"
                        //      },
                        //      {
                        //        "t": "L",
                        //        "d": "7.75000536403203,0|18.5,0"
                        //      }
                        //    ]
                        //  ]
                        // }
                        entity = null;
                        break;
                    case "L":
                        entity = jo.ToObject<JsonLine>(serializer);
                        break;
                    default:
                        entity = null;
                        break;
                }

                entity?.AfterParse();
                return entity;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
