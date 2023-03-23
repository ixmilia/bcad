using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Core.Test;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using Xunit;

namespace IxMilia.BCad.Rpc.Test
{
    public class PropertyPaneTests : TestBase
    {
        private Drawing GetDrawing(params Entity[] entities)
        {
            var testLayer = new Layer("test-layer");
            foreach (var entity in entities)
            {
                testLayer = testLayer.Add(entity);
            }

            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(new DimensionStyle("NON-STANDARD"))));
            drawing = drawing.Add(testLayer);
            return drawing;
        }

        private Dictionary<string, ClientPropertyPaneValue> GetEntityProperties(Drawing drawing, params Entity[] entities)
        {
            var propertyMap = drawing.GetPropertyPaneValues(entities).ToDictionary(cp => cp.Name);

            Assert.Equal(new ClientPropertyPaneValue("layer", "Layer", "test-layer", new[] { "0", "test-layer" }), propertyMap["layer"]);
            Assert.True(propertyMap.Remove("layer"));

            Assert.Equal("Color", propertyMap["color"].DisplayName);
            Assert.True(propertyMap.Remove("color"));

            Assert.Equal("Line Type", propertyMap["lineType"].DisplayName);
            Assert.True(propertyMap.Remove("lineType"));

            Assert.Equal("Line Type Scale", propertyMap["lineTypeScale"].DisplayName);
            Assert.True(propertyMap.Remove("lineTypeScale"));

            return propertyMap;
        }

        private (Drawing, Dictionary<string, ClientPropertyPaneValue>) GetDrawingAndEntityProperties(params Entity[] entities)
        {
            var drawing = GetDrawing(entities);
            var propertyMap = GetEntityProperties(drawing, entities);
            return (drawing, propertyMap);
        }

        private Dictionary<string, ClientPropertyPaneValue> GetEntityProperties(params Entity[] entities)
        {
            var (_drawing, propertyMap) = GetDrawingAndEntityProperties(entities);
            return propertyMap;
        }

        private TEntity DoUpdate<TEntity>(TEntity entity, string propertyName, string valueToSet) where TEntity : Entity
        {
            var (drawing, propertyMap) = GetDrawingAndEntityProperties(entity);
            var specificProperty = propertyMap[propertyName];
            Assert.True(specificProperty.TryDoUpdate(drawing, entity, valueToSet, out var updatedDrawingAndEntity));
            Assert.NotNull(updatedDrawingAndEntity.Item1);
            Assert.NotNull(updatedDrawingAndEntity.Item2);
            return (TEntity)updatedDrawingAndEntity.Item2;
        }

        [Fact]
        public void SetEntityCommonPropertyLayer()
        {
            var entity = new Location(new Point(0.0, 0.0, 0.0));
            var drawing = new Drawing().Add(new Layer("other-test-layer").Add(entity));
            var propertyMap = drawing.GetPropertyPaneValues(new[] { entity }).ToDictionary(cp => cp.Name);

            Assert.True(propertyMap["layer"].TryDoUpdate(drawing, entity, "other-test-layer", out var updatedDrawingAndEntity));
            Assert.Null(updatedDrawingAndEntity.Item2);
            Assert.Equal("other-test-layer", updatedDrawingAndEntity.Item1.ContainingLayer(entity).Name);
        }

        [Theory]
        [InlineData("#FF0000", "#0000FF")]
        [InlineData("#FF0000", null)]
        [InlineData(null, "#FF0000")]
        [InlineData(null, null)]
        public void SetEntityCommonPropertyColor(string initialColor, string targetColor)
        {
            CadColor? color = initialColor is null ? null : CadColor.Parse(initialColor);
            var entity = new Location(new Point(0.0, 0.0, 0.0), color: color);
            var drawing = new Drawing().Add(new Layer("test-layer").Add(entity));
            var propertyMap = drawing.GetPropertyPaneValues(new[] { entity }).ToDictionary(cp => cp.Name);
            var specificProperty = propertyMap["color"];
            var wasColorChanged = specificProperty.TryDoUpdate(drawing, entity, targetColor, out var updatedDrawingAndEntity);
            var wasColorExpectedToChange = initialColor != targetColor;
            Assert.Equal(wasColorExpectedToChange, wasColorChanged);
            if (wasColorChanged)
            {
                Assert.NotNull(updatedDrawingAndEntity.Item2);
                Assert.Equal(targetColor, updatedDrawingAndEntity.Item2.Color?.ToRGBString());
            }
            else
            {
                Assert.Null(updatedDrawingAndEntity);
            }
        }

        [Fact]
        public void GetMultipleEntityPropertyPaneValuesCommonLayerCommonNullColor()
        {
            var e1 = new Location(new Point());
            var e2 = new Location(new Point());
            var drawing = new Drawing().Add(new Layer("test-layer").Add(e1).Add(e2));
            var propertyMap = drawing.GetPropertyPaneValues(new[] { e1, e2 }).ToDictionary(cp => cp.Name);

            Assert.Equal(4, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("layer", "Layer", "test-layer", new[] { "0", "test-layer" }), propertyMap["layer"]);
            Assert.Equal(new ClientPropertyPaneValue("color", "Color", null), propertyMap["color"]);
            Assert.Equal(new ClientPropertyPaneValue("lineType", "Line Type", null, new[] { "(Auto)" }), propertyMap["lineType"]);
            Assert.Equal(new ClientPropertyPaneValue("lineTypeScale", "Line Type Scale", "1.0000"), propertyMap["lineTypeScale"]);
        }

        [Fact]
        public void GetMultipleEntityPropertyPaneValuesCommonSpecificColor()
        {
            var colorValue = "#FF0000";
            var color = CadColor.Parse(colorValue);
            var e1 = new Location(new Point(), color: color);
            var e2 = new Location(new Point(), color: color);
            var drawing = new Drawing().AddToCurrentLayer(e1).AddToCurrentLayer(e2);
            var propertyMap = drawing.GetPropertyPaneValues(new[] { e1, e2 }).ToDictionary(cp => cp.Name);

            Assert.Equal(new ClientPropertyPaneValue("color", "Color", colorValue), propertyMap["color"]);
        }

        [Fact]
        public void GetMultipleEntityPropertyPaneValuesDifferentLayerDifferentColor()
        {
            var e1 = new Location(new Point());
            var e2 = new Location(new Point(), color: CadColor.Red);
            var layer1 = new Layer("test-layer-1").Add(e1);
            var layer2 = new Layer("test-layer-2").Add(e2);
            var drawing = new Drawing().Add(layer1).Add(layer2);
            var propertyMap = drawing.GetPropertyPaneValues(new[] { e1, e2 }).ToDictionary(cp => cp.Name);

            Assert.Equal(4, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("layer", "Layer", null, new[] { "0", "test-layer-1", "test-layer-2" }, isUnrepresentable: true), propertyMap["layer"]);
            Assert.Equal(new ClientPropertyPaneValue("color", "Color", null, isUnrepresentable: true), propertyMap["color"]);
            Assert.Equal(new ClientPropertyPaneValue("lineType", "Line Type", null, new[] { "(Auto)" }), propertyMap["lineType"]);
            Assert.Equal(new ClientPropertyPaneValue("lineTypeScale", "Line Type Scale", "1.0000"), propertyMap["lineTypeScale"]);
        }

        [Fact]
        public void GetArcPropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, new Vector(7.0, 8.0, 9.0), thickness: 10));
            Assert.Equal(15, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("cx", "Center X", "0'1\""), propertyMap["cx"]);
            Assert.Equal(new ClientPropertyPaneValue("cy", "Y", "0'2\""), propertyMap["cy"]);
            Assert.Equal(new ClientPropertyPaneValue("cz", "Z", "0'3\""), propertyMap["cz"]);
            Assert.Equal(new ClientPropertyPaneValue("r", "Radius", "0'4\""), propertyMap["r"]);
            Assert.Equal(new ClientPropertyPaneValue("sa", "Start Angle", "5"), propertyMap["sa"]);
            Assert.Equal(new ClientPropertyPaneValue("ea", "End Angle", "6"), propertyMap["ea"]);
            Assert.Equal(new ClientPropertyPaneValue("nx", "Normal X", "0'7\""), propertyMap["nx"]);
            Assert.Equal(new ClientPropertyPaneValue("ny", "Y", "0'8\""), propertyMap["ny"]);
            Assert.Equal(new ClientPropertyPaneValue("nz", "Z", "0'9\""), propertyMap["nz"]);
            Assert.Equal(new ClientPropertyPaneValue("t", "Thickness", "0'10\""), propertyMap["t"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Start Point", "(0'4-7/16\",0'0-15/16\",0'5-3/16\")"), propertyMap["Start Point"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("End Point", "(0'4-7/16\",0'1\",0'5-1/4\")"), propertyMap["End Point"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Total Angle", "1"), propertyMap["Total Angle"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Arc Length", "0'0-1/16\""), propertyMap["Arc Length"]);
            Assert.Equal(new ClientPropertyPaneValue(false, true, "cc", "Convert to circle", null), propertyMap["cc"]);
        }

        [Theory]
        [InlineData("cx", "9", 9, 2, 3, 4, 5, 6, 0, 0, 1, 0)]
        [InlineData("cy", "9", 1, 9, 3, 4, 5, 6, 0, 0, 1, 0)]
        [InlineData("cz", "9", 1, 2, 9, 4, 5, 6, 0, 0, 1, 0)]
        [InlineData("r", "9", 1, 2, 3, 9, 5, 6, 0, 0, 1, 0)]
        [InlineData("sa", "9", 1, 2, 3, 4, 9, 6, 0, 0, 1, 0)]
        [InlineData("ea", "9", 1, 2, 3, 4, 5, 9, 0, 0, 1, 0)]
        [InlineData("nx", "9", 1, 2, 3, 4, 5, 6, 9, 0, 1, 0)]
        [InlineData("ny", "9", 1, 2, 3, 4, 5, 6, 0, 9, 1, 0)]
        [InlineData("nz", "9", 1, 2, 3, 4, 5, 6, 0, 0, 9, 0)]
        [InlineData("t", "9", 1, 2, 3, 4, 5, 6, 0, 0, 1, 9)]
        public void SetArcPropertyPaneValue(string propertyName, string propertyValue, double cx, double cy, double cz, double r, double sa, double ea, double nx, double ny, double nz, double t)
        {
            var entity = new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, new Vector(0.0, 0.0, 1.0), thickness: t);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(cx, cy, cz), finalEntity.Center);
            AssertClose(r, finalEntity.Radius);
            AssertClose(sa, finalEntity.StartAngle);
            AssertClose(ea, finalEntity.EndAngle);
            Assert.Equal(new Vector(nx, ny, nz), finalEntity.Normal);
            Assert.Equal(t, finalEntity.Thickness);
        }

        [Fact]
        public void GetCirclePropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Circle(new Point(1.0, 2.0, 3.0), 4.0, new Vector(5.0, 6.0, 7.0), thickness: 8));
            Assert.Equal(12, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("cx", "Center X", "0'1\""), propertyMap["cx"]);
            Assert.Equal(new ClientPropertyPaneValue("cy", "Y", "0'2\""), propertyMap["cy"]);
            Assert.Equal(new ClientPropertyPaneValue("cz", "Z", "0'3\""), propertyMap["cz"]);
            Assert.Equal(new ClientPropertyPaneValue("r", "Radius", "0'4\""), propertyMap["r"]);
            Assert.Equal(new ClientPropertyPaneValue("nx", "Normal X", "0'5\""), propertyMap["nx"]);
            Assert.Equal(new ClientPropertyPaneValue("ny", "Y", "0'6\""), propertyMap["ny"]);
            Assert.Equal(new ClientPropertyPaneValue("nz", "Z", "0'7\""), propertyMap["nz"]);
            Assert.Equal(new ClientPropertyPaneValue("t", "Thickness", "0'8\""), propertyMap["t"]);
            Assert.Equal(new ClientPropertyPaneValue("a", "Area", "50.2655"), propertyMap["a"]);
            Assert.Equal(new ClientPropertyPaneValue("d", "Diameter", "0'8\""), propertyMap["d"]);
            Assert.Equal(new ClientPropertyPaneValue("c", "Circumference", "2'1-1/8\""), propertyMap["c"]);
            Assert.Equal(new ClientPropertyPaneValue(false, true, "ca", "Convert to arc", null), propertyMap["ca"]);
        }

        [Theory]
        [InlineData("cx", "9", 9, 2, 3, 4, 0, 0, 1, 0)]
        [InlineData("cy", "9", 1, 9, 3, 4, 0, 0, 1, 0)]
        [InlineData("cz", "9", 1, 2, 9, 4, 0, 0, 1, 0)]
        [InlineData("r", "9", 1, 2, 3, 9, 0, 0, 1, 0)]
        [InlineData("nx", "9", 1, 2, 3, 4, 9, 0, 1, 0)]
        [InlineData("ny", "9", 1, 2, 3, 4, 0, 9, 1, 0)]
        [InlineData("nz", "9", 1, 2, 3, 4, 0, 0, 9, 0)]
        [InlineData("t", "9", 1, 2, 3, 4, 0, 0, 1, 9)]
        public void SetCirclePropertyPaneValue(string propertyName, string propertyValue, double cx, double cy, double cz, double r, double nx, double ny, double nz, double t)
        {
            var entity = new Circle(new Point(1.0, 2.0, 3.0), 4.0, new Vector(0.0, 0.0, 1.0), thickness: t);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(cx, cy, cz), finalEntity.Center);
            AssertClose(r, finalEntity.Radius);
            Assert.Equal(new Vector(nx, ny, nz), finalEntity.Normal);
            Assert.Equal(t, finalEntity.Thickness);
        }

        [Fact]
        public void GetEllipsePropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Ellipse(new Point(1.0, 2.0, 3.0), new Vector(4.0, 5.0, 6.0), 7.0, 8.0, 9.0, new Vector(10.0, 11.0, 12.0), thickness: 13));
            Assert.Equal(13, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("cx", "Center X", "0'1\""), propertyMap["cx"]);
            Assert.Equal(new ClientPropertyPaneValue("cy", "Y", "0'2\""), propertyMap["cy"]);
            Assert.Equal(new ClientPropertyPaneValue("cz", "Z", "0'3\""), propertyMap["cz"]);
            Assert.Equal(new ClientPropertyPaneValue("mx", "Major Axis X", "0'4\""), propertyMap["mx"]);
            Assert.Equal(new ClientPropertyPaneValue("my", "Y", "0'5\""), propertyMap["my"]);
            Assert.Equal(new ClientPropertyPaneValue("mz", "Z", "0'6\""), propertyMap["mz"]);
            Assert.Equal(new ClientPropertyPaneValue("mr", "Minor Axis Ratio", "7.0000"), propertyMap["mr"]);
            Assert.Equal(new ClientPropertyPaneValue("sa", "Start Angle", "8"), propertyMap["sa"]);
            Assert.Equal(new ClientPropertyPaneValue("ea", "End Angle", "9"), propertyMap["ea"]);
            Assert.Equal(new ClientPropertyPaneValue("nx", "Normal X", "0'10\""), propertyMap["nx"]);
            Assert.Equal(new ClientPropertyPaneValue("ny", "Y", "0'11\""), propertyMap["ny"]);
            Assert.Equal(new ClientPropertyPaneValue("nz", "Z", "1'0\""), propertyMap["nz"]);
            Assert.Equal(new ClientPropertyPaneValue("t", "Thickness", "1'1\""), propertyMap["t"]);
        }

        [Theory]
        [InlineData("cx", "99", 99, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("cy", "99", 1, 99, 3, 4, 5, 6, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("cz", "99", 1, 2, 99, 4, 5, 6, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("mx", "99", 1, 2, 3, 99, 5, 6, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("my", "99", 1, 2, 3, 4, 99, 6, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("mz", "99", 1, 2, 3, 4, 5, 99, 7, 8, 9, 0, 0, 1, 0)]
        [InlineData("mr", "99", 1, 2, 3, 4, 5, 6, 99, 8, 9, 0, 0, 1, 0)]
        [InlineData("sa", "99", 1, 2, 3, 4, 5, 6, 7, 99, 9, 0, 0, 1, 0)]
        [InlineData("ea", "99", 1, 2, 3, 4, 5, 6, 7, 8, 99, 0, 0, 1, 0)]
        [InlineData("nx", "99", 1, 2, 3, 4, 5, 6, 7, 8, 9, 99, 0, 1, 0)]
        [InlineData("ny", "99", 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 99, 1, 0)]
        [InlineData("nz", "99", 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 99, 0)]
        [InlineData("t", "99", 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 1, 99)]
        public void SetEllipsePropertyPaneValue(string propertyName, string propertyValue, double cx, double cy, double cz, double mx, double my, double mz, double ma, double sa, double ea, double nx, double ny, double nz, double t)
        {
            var entity = new Ellipse(new Point(1.0, 2.0, 3.0), new Vector(4.0, 5.0, 6.0), 7.0, 8.0, 9.0, new Vector(0.0, 0.0, 1.0), thickness: t);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(cx, cy, cz), finalEntity.Center);
            Assert.Equal(new Vector(mx, my, mz), finalEntity.MajorAxis);
            AssertClose(ma, finalEntity.MinorAxisRatio);
            AssertClose(sa, finalEntity.StartAngle);
            AssertClose(ea, finalEntity.EndAngle);
            Assert.Equal(new Vector(nx, ny, nz), finalEntity.Normal);
            Assert.Equal(t, finalEntity.Thickness);
        }

        [Fact]
        public void GetImagePropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Image(new Point(1.0, 2.0, 3.0), "some-path", Array.Empty<byte>(), 4.0, 5.0, 6.0));
            Assert.Equal(7, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("x", "Location X", "0'1\""), propertyMap["x"]);
            Assert.Equal(new ClientPropertyPaneValue("y", "Y", "0'2\""), propertyMap["y"]);
            Assert.Equal(new ClientPropertyPaneValue("z", "Z", "0'3\""), propertyMap["z"]);
            Assert.Equal(new ClientPropertyPaneValue("p", "Path", "some-path"), propertyMap["p"]);
            Assert.Equal(new ClientPropertyPaneValue("w", "Width", "0'4\""), propertyMap["w"]);
            Assert.Equal(new ClientPropertyPaneValue("h", "Height", "0'5\""), propertyMap["h"]);
            Assert.Equal(new ClientPropertyPaneValue("r", "Rotation", "6"), propertyMap["r"]);
        }

        [Theory]
        [InlineData("x", "9", 9, 2, 3, "some-path", 4, 5, 6)]
        [InlineData("y", "9", 1, 9, 3, "some-path", 4, 5, 6)]
        [InlineData("z", "9", 1, 2, 9, "some-path", 4, 5, 6)]
        [InlineData("p", "9", 1, 2, 3, "9", 4, 5, 6)]
        [InlineData("w", "9", 1, 2, 3, "some-path", 9, 5, 6)]
        [InlineData("h", "9", 1, 2, 3, "some-path", 4, 9, 6)]
        [InlineData("r", "9", 1, 2, 3, "some-path", 4, 5, 9)]
        public void SetImagePropertyPaneValue(string propertyName, string propertyValue, double x, double y, double z, string p, double w, double h, double r)
        {
            var entity = new Image(new Point(1.0, 2.0, 3.0), "some-path", Array.Empty<byte>(), 4.0, 5.0, 6.0);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(x, y, z), finalEntity.Location);
            Assert.Equal(p, finalEntity.Path);
            Assert.Equal(w, finalEntity.Width);
            Assert.Equal(h, finalEntity.Height);
            Assert.Equal(r, finalEntity.Rotation);
        }

        [Fact]
        public void GetLinearDimensionPropertyPaneValue()
        {
            var entity = new LinearDimension(
                new Point(1.0, 2.0, 0.0),
                new Point(3.0, 4.0, 0.0),
                new Point(5.0, 6.0, 0.0),
                true,
                new Point(7.0, 8.0, 0.0),
                "NON-STANDARD",
                "text-override",
                CadColor.Green);
            var drawing = GetDrawing(entity);
            var _primitives = entity.GetPrimitives(drawing.Settings); // need to force this for certain computed properties
            var propertyMap = GetEntityProperties(drawing, entity);
            Assert.Equal(17, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("ds", "Dimension Style", "NON-STANDARD", new[] { "NON-STANDARD", "STANDARD" }), propertyMap["ds"]);
            Assert.Equal(new ClientPropertyPaneValue("a", "Kind", "Aligned", new[] { "Aligned", "Non-Aligned" }), propertyMap["a"]);
            Assert.Equal(new ClientPropertyPaneValue("x1", "Definition Point 1 X", "0'1\""), propertyMap["x1"]);
            Assert.Equal(new ClientPropertyPaneValue("y1", "Y", "0'2\""), propertyMap["y1"]);
            Assert.Equal(new ClientPropertyPaneValue("z1", "Z", "0'0\""), propertyMap["z1"]);
            Assert.Equal(new ClientPropertyPaneValue("x2", "Definition Point 2 X", "0'3\""), propertyMap["x2"]);
            Assert.Equal(new ClientPropertyPaneValue("y2", "Y", "0'4\""), propertyMap["y2"]);
            Assert.Equal(new ClientPropertyPaneValue("z2", "Z", "0'0\""), propertyMap["z2"]);
            Assert.Equal(new ClientPropertyPaneValue("x3", "Line Location X", "0'5\""), propertyMap["x3"]);
            Assert.Equal(new ClientPropertyPaneValue("y3", "Y", "0'6\""), propertyMap["y3"]);
            Assert.Equal(new ClientPropertyPaneValue("z3", "Z", "0'0\""), propertyMap["z3"]);
            Assert.Equal(new ClientPropertyPaneValue("tx", "Text Midpoint X", "0'7\""), propertyMap["tx"]);
            Assert.Equal(new ClientPropertyPaneValue("ty", "Y", "0'8\""), propertyMap["ty"]);
            Assert.Equal(new ClientPropertyPaneValue("tz", "Z", "0'0\""), propertyMap["tz"]);
            Assert.Equal(new ClientPropertyPaneValue("to", "Text Override", "text-override"), propertyMap["to"]);
            Assert.Equal(new ClientPropertyPaneValue("t-color", "Text Color", "#00FF00"), propertyMap["t-color"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Actual Measurement", "0'2-13/16\""), propertyMap["Actual Measurement"]);
        }

        [Theory]
        [InlineData("ds", "NON-STANDARD", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "NON-STANDARD", "text-override", "#FF0000")]
        [InlineData("x1", "9", 9.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("y1", "9", 1.0, 9.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("z1", "9", 1.0, 2.0, 9.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("x2", "9", 1.0, 2.0, 0.0, 9.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("y2", "9", 1.0, 2.0, 0.0, 3.0, 9.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("z2", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 9.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("x3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 9.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("y3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 9.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("z3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 9.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("a", "Non-Aligned", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, false, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("tx", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 9.0, 8.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("ty", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 9.0, 0.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("tz", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 9.0, "STANDARD", "text-override", "#FF0000")]
        [InlineData("to", "new-text-override", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "new-text-override", "#FF0000")]
        [InlineData("t-color", "#0000FF", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, true, 7.0, 8.0, 0.0, "STANDARD", "text-override", "#0000FF")]
        public void SetLinearDimensionPropertyPaneValue(
            string propertyName,
            string propertyValue,
            double dp1x, double dp1y, double dp1z,
            double dp2x, double dp2y, double dp2z,
            double dllx, double dlly, double dllz,
            bool isAligned,
            double tmpx, double tmpy, double tmpz,
            string dimStyleName,
            string textOverride,
            string textColor)
        {
            var entity = new LinearDimension(new Point(1.0, 2.0, 0.0), new Point(3.0, 4.0, 0.0), new Point(5.0, 6.0, 0.0), true, new Point(7.0, 8.0, 0.0), "STANDARD", "text-override", textColor: CadColor.Red);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(dp1x, dp1y, dp1z), finalEntity.DefinitionPoint1);
            Assert.Equal(new Point(dp2x, dp2y, dp2z), finalEntity.DefinitionPoint2);
            Assert.Equal(new Point(dllx, dlly, dllz), finalEntity.DimensionLineLocation);
            Assert.Equal(isAligned, finalEntity.IsAligned);
            Assert.Equal(new Point(tmpx, tmpy, tmpz), finalEntity.TextMidPoint);
            Assert.Equal(dimStyleName, finalEntity.DimensionStyleName);
            Assert.Equal(textOverride, finalEntity.TextOverride);
            Assert.Equal(CadColor.Parse(textColor), finalEntity.TextColor);
        }

        [Fact]
        public void GetLinePropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), thickness: 7));
            Assert.Equal(10, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("x1", "Start X", "0'1\""), propertyMap["x1"]);
            Assert.Equal(new ClientPropertyPaneValue("y1", "Y", "0'2\""), propertyMap["y1"]);
            Assert.Equal(new ClientPropertyPaneValue("z1", "Z", "0'3\""), propertyMap["z1"]);
            Assert.Equal(new ClientPropertyPaneValue("x2", "End X", "0'4\""), propertyMap["x2"]);
            Assert.Equal(new ClientPropertyPaneValue("y2", "Y", "0'5\""), propertyMap["y2"]);
            Assert.Equal(new ClientPropertyPaneValue("z2", "Z", "0'6\""), propertyMap["z2"]);
            Assert.Equal(new ClientPropertyPaneValue("t", "Thickness", "0'7\""), propertyMap["t"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Length", "0'5-3/16\""), propertyMap["Length"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Delta", "(0'3\",0'3\",0'3\")"), propertyMap["Delta"]);
            Assert.Equal(ClientPropertyPaneValue.CreateReadOnly("Angle", "45"), propertyMap["Angle"]);
        }

        [Theory]
        [InlineData("x1", "9", 9, 2, 3, 4, 5, 6, 0)]
        [InlineData("y1", "9", 1, 9, 3, 4, 5, 6, 0)]
        [InlineData("z1", "9", 1, 2, 9, 4, 5, 6, 0)]
        [InlineData("x2", "9", 1, 2, 3, 9, 5, 6, 0)]
        [InlineData("y2", "9", 1, 2, 3, 4, 9, 6, 0)]
        [InlineData("z2", "9", 1, 2, 3, 4, 5, 9, 0)]
        [InlineData("t", "9", 1, 2, 3, 4, 5, 6, 9)]
        public void SetLinePropertyPaneValue(string propertyName, string propertyValue, double x1, double y1, double z1, double x2, double y2, double z2, double t)
        {
            var entity = new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), thickness: t);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(x1, y1, z1), finalEntity.P1);
            Assert.Equal(new Point(x2, y2, z2), finalEntity.P2);
            Assert.Equal(t, finalEntity.Thickness);
        }

        [Fact]
        public void GetLocationPropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Location(new Point(1.0, 2.0, 3.0)));
            Assert.Equal(3, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("x", "Location X", "0'1\""), propertyMap["x"]);
            Assert.Equal(new ClientPropertyPaneValue("y", "Y", "0'2\""), propertyMap["y"]);
            Assert.Equal(new ClientPropertyPaneValue("z", "Z", "0'3\""), propertyMap["z"]);
        }

        [Theory]
        [InlineData("x", "9", 9, 2, 3)]
        [InlineData("y", "9", 1, 9, 3)]
        [InlineData("z", "9", 1, 2, 9)]
        public void SetLocationPropertyPaneValue(string propertyName, string propertyValue, double x, double y, double z)
        {
            var entity = new Location(new Point(1.0, 2.0, 3.0));
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(x, y, z), finalEntity.Point);
        }

        [Fact]
        public void GetSolidPropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Solid(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), new Point(7.0, 8.0, 9.0), new Point(10.0, 11.0, 12.0)));
            Assert.Equal(12, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("x1", "Point 1 X", "0'1\""), propertyMap["x1"]);
            Assert.Equal(new ClientPropertyPaneValue("y1", "Y", "0'2\""), propertyMap["y1"]);
            Assert.Equal(new ClientPropertyPaneValue("z1", "Z", "0'3\""), propertyMap["z1"]);
            Assert.Equal(new ClientPropertyPaneValue("x2", "Point 2 X", "0'4\""), propertyMap["x2"]);
            Assert.Equal(new ClientPropertyPaneValue("y2", "Y", "0'5\""), propertyMap["y2"]);
            Assert.Equal(new ClientPropertyPaneValue("z2", "Z", "0'6\""), propertyMap["z2"]);
            Assert.Equal(new ClientPropertyPaneValue("x3", "Point 3 X", "0'7\""), propertyMap["x3"]);
            Assert.Equal(new ClientPropertyPaneValue("y3", "Y", "0'8\""), propertyMap["y3"]);
            Assert.Equal(new ClientPropertyPaneValue("z3", "Z", "0'9\""), propertyMap["z3"]);
            Assert.Equal(new ClientPropertyPaneValue("x4", "Point 4 X", "0'10\""), propertyMap["x4"]);
            Assert.Equal(new ClientPropertyPaneValue("y4", "Y", "0'11\""), propertyMap["y4"]);
            Assert.Equal(new ClientPropertyPaneValue("z4", "Z", "1'0\""), propertyMap["z4"]);
        }

        [Theory]
        [InlineData("x1", "9", 9.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("y1", "9", 1.0, 9.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("z1", "9", 1.0, 2.0, 9.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("x2", "9", 1.0, 2.0, 0.0, 9.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("y2", "9", 1.0, 2.0, 0.0, 3.0, 9.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("z2", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 9.0, 5.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("x3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 9.0, 6.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("y3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 9.0, 0.0, 7.0, 8.0, 0.0)]
        [InlineData("z3", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 9.0, 7.0, 8.0, 0.0)]
        [InlineData("x4", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 9.0, 8.0, 0.0)]
        [InlineData("y4", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 9.0, 0.0)]
        [InlineData("z4", "9", 1.0, 2.0, 0.0, 3.0, 4.0, 0.0, 5.0, 6.0, 0.0, 7.0, 8.0, 9.0)]
        public void SetSolidPropertyPaneValue(
            string propertyName,
            string propertyValue,
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            double x3, double y3, double z3,
            double x4, double y4, double z4)
        {
            var entity = new Solid(new Point(1.0, 2.0, 0.0), new Point(3.0, 4.0, 0.0), new Point(5.0, 6.0, 0.0), new Point(7.0, 8.0, 0.0));
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(new Point(x1, y1, z1), finalEntity.P1);
            Assert.Equal(new Point(x2, y2, z2), finalEntity.P2);
            Assert.Equal(new Point(x3, y3, z3), finalEntity.P3);
            Assert.Equal(new Point(x4, y4, z4), finalEntity.P4);
        }

        [Fact]
        public void GetTextPropertyPaneValue()
        {
            var propertyMap = GetEntityProperties(new Text("the-value", new Point(1.0, 2.0, 3.0), new Vector(0.0, 0.0, 1.0), 4.0, 5.0));
            Assert.Equal(9, propertyMap.Count);
            Assert.Equal(new ClientPropertyPaneValue("v", "Value", "the-value"), propertyMap["v"]);
            Assert.Equal(new ClientPropertyPaneValue("x", "Location X", "0'1\""), propertyMap["x"]);
            Assert.Equal(new ClientPropertyPaneValue("y", "Y", "0'2\""), propertyMap["y"]);
            Assert.Equal(new ClientPropertyPaneValue("z", "Z", "0'3\""), propertyMap["z"]);
            Assert.Equal(new ClientPropertyPaneValue("h", "Height", "0'4\""), propertyMap["h"]);
            Assert.Equal(new ClientPropertyPaneValue("r", "Rotation", "5"), propertyMap["r"]);
            Assert.Equal(new ClientPropertyPaneValue("nx", "Normal X", "0'0\""), propertyMap["nx"]);
            Assert.Equal(new ClientPropertyPaneValue("ny", "Y", "0'0\""), propertyMap["ny"]);
            Assert.Equal(new ClientPropertyPaneValue("nz", "Z", "0'1\""), propertyMap["nz"]);
        }

        [Theory]
        [InlineData("v", "9", "9", 1, 2, 3, 4, 5, 0, 0, 1)]
        [InlineData("x", "9", "the-value", 9, 2, 3, 4, 5, 0, 0, 1)]
        [InlineData("y", "9", "the-value", 1, 9, 3, 4, 5, 0, 0, 1)]
        [InlineData("z", "9", "the-value", 1, 2, 9, 4, 5, 0, 0, 1)]
        [InlineData("h", "9", "the-value", 1, 2, 3, 9, 5, 0, 0, 1)]
        [InlineData("r", "9", "the-value", 1, 2, 3, 4, 9, 0, 0, 1)]
        [InlineData("nx", "9", "the-value", 1, 2, 3, 4, 5, 9, 0, 1)]
        [InlineData("ny", "9", "the-value", 1, 2, 3, 4, 5, 0, 9, 1)]
        [InlineData("nz", "9", "the-value", 1, 2, 3, 4, 5, 0, 0, 9)]
        public void SetTextPropertyPaneValue(string propertyName, string propertyValue, string value, double x, double y, double z, double h, double r, double nx, double ny, double nz)
        {
            var entity = new Text("the-value", new Point(1.0, 2.0, 3.0), new Vector(nx, ny, nz), h, r);
            var finalEntity = DoUpdate(entity, propertyName, propertyValue);
            Assert.Equal(value, finalEntity.Value);
            Assert.Equal(new Point(x, y, z), finalEntity.Location);
            Assert.Equal(h, finalEntity.Height);
            Assert.Equal(r, finalEntity.Rotation);
            Assert.Equal(new Vector(nx, ny, nz), finalEntity.Normal);
        }

        [Fact]
        public void ConvertArcToCircle()
        {
            var arc = new Arc(new Point(1.0, 2.0, 3.0), 1.0, 90.0, 180.0, Vector.ZAxis);
            var (drawing, propertyMap) = GetDrawingAndEntityProperties(arc);
            var converter = propertyMap["cc"];
            Assert.True(converter.TryDoUpdate(drawing, arc, null, out var result));
            var entities = result.Item1.GetEntities().ToList();
            Assert.Empty(entities.OfType<Arc>());
            var onlyCircle = entities.OfType<Circle>().Single();
            var circle = Assert.IsType<Circle>(result.Item2);
            Assert.Same(circle, onlyCircle);
            Assert.Equal(arc.Center, circle.Center);
            Assert.Equal(arc.Radius, circle.Radius);
            Assert.Equal(arc.Normal, circle.Normal);
        }

        [Fact]
        public void ConvertCircleToArc()
        {
            var circle = new Circle(new Point(1.0, 2.0, 3.0), 1.0, Vector.ZAxis);
            var (drawing, propertyMap) = GetDrawingAndEntityProperties(circle);
            var converter = propertyMap["ca"];
            Assert.True(converter.TryDoUpdate(drawing, circle, null, out var result));
            var entities = result.Item1.GetEntities().ToList();
            Assert.Empty(entities.OfType<Circle>());
            var onlyArc = entities.OfType<Arc>().Single();
            var arc = Assert.IsType<Arc>(result.Item2);
            Assert.Same(arc, onlyArc);
            Assert.Equal(circle.Center, arc.Center);
            Assert.Equal(circle.Radius, arc.Radius);
            Assert.Equal(circle.Normal, arc.Normal);
            Assert.Equal(0.0, arc.StartAngle);
            Assert.Equal(360.0, arc.EndAngle);
        }
    }
}
