using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Rpc
{
    public static class PropertyExtensions
    {
        private static IEnumerable<string> GetAllLayerNames(this Drawing drawing) => drawing.GetLayers().Select(l => l.Name).OrderBy(s => s);

        private static string ToPropertyColorString(this CadColor? color) => color?.ToRGBString();

        public static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValuesForMultipleEntities(this Drawing drawing, IEnumerable<Entity> entities)
        {
            var distinctLayerNames = entities.Select(e => drawing.ContainingLayer(e)).Select(l => l.Name).Distinct().ToList();
            var selectedLayerName = distinctLayerNames.Count == 1
                ? distinctLayerNames.Single()
                : null;

            yield return new ClientPropertyPaneValue("layer", "Layer", selectedLayerName, drawing.GetAllLayerNames(), isUnrepresentable: distinctLayerNames.Count > 1);

            var distinctColors = entities.Select(e => e.Color).Select(c => c.ToPropertyColorString()).Distinct().ToList();
            var selectedColor = distinctColors.Count == 1
                ? distinctColors.Single()
                : null;

            yield return new ClientPropertyPaneValue("color", "Color", selectedColor, isUnrepresentable: distinctColors.Count > 1);
        }

        public static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValues(this Drawing drawing, Entity entity)
        {
            var layer = drawing.ContainingLayer(entity);

            yield return new ClientPropertyPaneValue("layer", "Layer", layer.Name, drawing.GetAllLayerNames());
            yield return new ClientPropertyPaneValue("color", "Color", entity.Color.ToPropertyColorString());

            switch (entity)
            {
                case Arc arc:
                    yield return new ClientPropertyPaneValue("cx", "Center X", drawing.FormatUnits(arc.Center.X));
                    yield return new ClientPropertyPaneValue("cy", "Y", drawing.FormatUnits(arc.Center.Y));
                    yield return new ClientPropertyPaneValue("cz", "Z", drawing.FormatUnits(arc.Center.Z));
                    yield return new ClientPropertyPaneValue("r", "Radius", drawing.FormatUnits(arc.Radius));
                    yield return new ClientPropertyPaneValue("sa", "Start Angle", DrawingSettings.FormatUnits(arc.StartAngle, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("ea", "End Angle", DrawingSettings.FormatUnits(arc.EndAngle, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("nx", "Normal X", drawing.FormatUnits(arc.Normal.X));
                    yield return new ClientPropertyPaneValue("ny", "Y", drawing.FormatUnits(arc.Normal.Y));
                    yield return new ClientPropertyPaneValue("nz", "Z", drawing.FormatUnits(arc.Normal.Z));
                    yield return new ClientPropertyPaneValue("t", "Thickness", drawing.FormatUnits(arc.Thickness));
                    break;
                case Circle circle:
                    yield return new ClientPropertyPaneValue("cx", "Center X", drawing.FormatUnits(circle.Center.X));
                    yield return new ClientPropertyPaneValue("cy", "Y", drawing.FormatUnits(circle.Center.Y));
                    yield return new ClientPropertyPaneValue("cz", "Z", drawing.FormatUnits(circle.Center.Z));
                    yield return new ClientPropertyPaneValue("r", "Radius", drawing.FormatUnits(circle.Radius));
                    yield return new ClientPropertyPaneValue("nx", "Normal X", drawing.FormatUnits(circle.Normal.X));
                    yield return new ClientPropertyPaneValue("ny", "Y", drawing.FormatUnits(circle.Normal.Y));
                    yield return new ClientPropertyPaneValue("nz", "Z", drawing.FormatUnits(circle.Normal.Z));
                    yield return new ClientPropertyPaneValue("t", "Thickness", drawing.FormatUnits(circle.Thickness));
                    break;
                case Ellipse el:
                    yield return new ClientPropertyPaneValue("cx", "Center X", drawing.FormatUnits(el.Center.X));
                    yield return new ClientPropertyPaneValue("cy", "Y", drawing.FormatUnits(el.Center.Y));
                    yield return new ClientPropertyPaneValue("cz", "Z", drawing.FormatUnits(el.Center.Z));
                    yield return new ClientPropertyPaneValue("mx", "Major Axis X", drawing.FormatUnits(el.MajorAxis.X));
                    yield return new ClientPropertyPaneValue("my", "Y", drawing.FormatUnits(el.MajorAxis.Y));
                    yield return new ClientPropertyPaneValue("mz", "Z", drawing.FormatUnits(el.MajorAxis.Z));
                    yield return new ClientPropertyPaneValue("mr", "Minor Axis Ratio", DrawingSettings.FormatUnits(el.MinorAxisRatio, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("sa", "Start Angle", DrawingSettings.FormatUnits(el.StartAngle, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("ea", "End Angle", DrawingSettings.FormatUnits(el.EndAngle, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("nx", "Normal X", drawing.FormatUnits(el.Normal.X));
                    yield return new ClientPropertyPaneValue("ny", "Y", drawing.FormatUnits(el.Normal.Y));
                    yield return new ClientPropertyPaneValue("nz", "Z", drawing.FormatUnits(el.Normal.Z));
                    yield return new ClientPropertyPaneValue("t", "Thickness", drawing.FormatUnits(el.Thickness));
                    break;
                case Line line:
                    yield return new ClientPropertyPaneValue("x1", "Start X", drawing.FormatUnits(line.P1.X));
                    yield return new ClientPropertyPaneValue("y1", "Y", drawing.FormatUnits(line.P1.Y));
                    yield return new ClientPropertyPaneValue("z1", "Z", drawing.FormatUnits(line.P1.Z));
                    yield return new ClientPropertyPaneValue("x2", "End X", drawing.FormatUnits(line.P2.X));
                    yield return new ClientPropertyPaneValue("y2", "Y", drawing.FormatUnits(line.P2.Y));
                    yield return new ClientPropertyPaneValue("z2", "Z", drawing.FormatUnits(line.P2.Z));
                    yield return new ClientPropertyPaneValue("t", "Thickness", drawing.FormatUnits(line.Thickness));
                    break;
                case Location loc:
                    yield return new ClientPropertyPaneValue("x", "Location X", drawing.FormatUnits(loc.Point.X));
                    yield return new ClientPropertyPaneValue("y", "Y", drawing.FormatUnits(loc.Point.Y));
                    yield return new ClientPropertyPaneValue("z", "Z", drawing.FormatUnits(loc.Point.Z));
                    break;
                case Text text:
                    yield return new ClientPropertyPaneValue("v", "Value", text.Value);
                    yield return new ClientPropertyPaneValue("x", "Location X", drawing.FormatUnits(text.Location.X));
                    yield return new ClientPropertyPaneValue("y", "Y", drawing.FormatUnits(text.Location.Y));
                    yield return new ClientPropertyPaneValue("z", "Z", drawing.FormatUnits(text.Location.Z));
                    yield return new ClientPropertyPaneValue("h", "Height", drawing.FormatUnits(text.Height));
                    yield return new ClientPropertyPaneValue("r", "Rotation", DrawingSettings.FormatUnits(text.Rotation, UnitFormat.Metric, drawing.Settings.UnitPrecision));
                    yield return new ClientPropertyPaneValue("nx", "Normal X", drawing.FormatUnits(text.Normal.X));
                    yield return new ClientPropertyPaneValue("ny", "Y", drawing.FormatUnits(text.Normal.Y));
                    yield return new ClientPropertyPaneValue("nz", "Z", drawing.FormatUnits(text.Normal.Z));
                    break;
                // TODO: other entities
            }
        }

        public static bool TrySetPropertyPaneValue(this Drawing drawing, Entity entity, ClientPropertyPaneValue value, out Drawing updatedDrawing, out Entity updatedEntity)
        {
            updatedDrawing = default;
            updatedEntity = default;
            if (value.Name == "layer")
            {
                var containingLayer = drawing.ContainingLayer(entity);
                if (containingLayer.Name == value.Value)
                {
                    // no change
                    return false;
                }

                var destinationLayer = drawing.Layers.GetValue(value.Value);
                var updatedContainingLayer = containingLayer.Remove(entity);
                var updatedDestinationLayer = destinationLayer.Add(entity);
                updatedDrawing = drawing
                    .Remove(containingLayer)
                    .Remove(destinationLayer)
                    .Add(updatedContainingLayer)
                    .Add(updatedDestinationLayer);
                return true;
            }

            if (entity.TrySetEntityPropertyPaneValue(value, out updatedEntity))
            {
                updatedDrawing = drawing.Replace(entity, updatedEntity);
                return true;
            }

            return false;
        }

        public static bool TrySetEntityPropertyPaneValue(this Entity entity, ClientPropertyPaneValue value, out Entity updatedEntity)
        {
            updatedEntity = default;
            if (value.Name == "color")
            {
                if (value.Value is null)
                {
                    // unset color
                    if (!entity.Color.HasValue)
                    {
                        // nothing to do
                        return false;
                    }
                    else
                    {
                        updatedEntity = entity.WithColor(null);
                        return true;
                    }
                }
                else
                {
                    // set color
                    var color = CadColor.Parse(value.Value);
                    if (entity.Color == color)
                    {
                        // nothing to do
                        return false;
                    }
                    else
                    {
                        updatedEntity = entity.WithColor(color);
                        return true;
                    }
                }
            }

            switch (entity)
            {
                case Arc arc:
                    if (arc.TrySetArcPropertyPaneValue(value, out var updatedArc))
                    {
                        updatedEntity = updatedArc;
                        return true;
                    }
                    break;
                case Circle circle:
                    if (circle.TrySetCirclePropertyPaneValue(value, out var updatedCircle))
                    {
                        updatedEntity = updatedCircle;
                        return true;
                    }
                    break;
                case Ellipse el:
                    if (el.TrySetEllipsePropertyPaneValue(value, out var updatedEllipse))
                    {
                        updatedEntity = updatedEllipse;
                        return true;
                    }
                    break;
                case Line line:
                    if (line.TrySetLinePropertyPaneValue(value, out var updatedLine))
                    {
                        updatedEntity = updatedLine;
                        return true;
                    }
                    break;
                case Location loc:
                    if (loc.TrySetLocationPropertyPaneValue(value, out var updatedLocation))
                    {
                        updatedEntity = updatedLocation;
                        return true;
                    }
                    break;
                case Text text:
                    if (text.TrySetTextPropertyPaneValue(value, out var updatedText))
                    {
                        updatedEntity = updatedText;
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static bool TrySetArcPropertyPaneValue(this Arc arc, ClientPropertyPaneValue value, out Arc updatedArc)
        {
            switch (value.Name)
            {
                case "cx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(center: arc.Center.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cy":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(center: arc.Center.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(center: arc.Center.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(normal: arc.Normal.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "ny":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(normal: arc.Normal.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(normal: arc.Normal.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "r":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(radius: unitValue);
                            return true;
                        }
                        break;
                    }
                case "sa":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(startAngle: unitValue);
                            return true;
                        }
                        break;
                    }
                case "ea":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(endAngle: unitValue);
                            return true;
                        }
                        break;
                    }
                case "t":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedArc = arc.Update(thickness: unitValue);
                            return true;
                        }
                        break;
                    }
            }

            updatedArc = default;
            return false;
        }

        private static bool TrySetCirclePropertyPaneValue(this Circle circle, ClientPropertyPaneValue value, out Circle updatedCircle)
        {
            switch (value.Name)
            {
                case "cx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(center: circle.Center.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cy":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(center: circle.Center.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(center: circle.Center.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(normal: circle.Normal.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "ny":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(normal: circle.Normal.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(normal: circle.Normal.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "r":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(radius: unitValue);
                            return true;
                        }
                        break;
                    }
                case "t":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedCircle = circle.Update(thickness: unitValue);
                            return true;
                        }
                        break;
                    }
            }

            updatedCircle = default;
            return false;
        }

        private static bool TrySetEllipsePropertyPaneValue(this Ellipse el, ClientPropertyPaneValue value, out Ellipse updatedEllipse)
        {
            switch (value.Name)
            {
                case "cx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(center: el.Center.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cy":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(center: el.Center.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "cz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(center: el.Center.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "mr":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(minorAxisRatio: unitValue);
                            return true;
                        }
                        break;
                    }
                case "mx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(majorAxis: el.MajorAxis.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "my":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(majorAxis: el.MajorAxis.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "mz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(majorAxis: el.MajorAxis.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(normal: el.Normal.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "ny":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(normal: el.Normal.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(normal: el.Normal.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "sa":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(startAngle: unitValue);
                            return true;
                        }
                        break;
                    }
                case "ea":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(endAngle: unitValue);
                            return true;
                        }
                        break;
                    }
                case "t":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedEllipse = el.Update(thickness: unitValue);
                            return true;
                        }
                        break;
                    }
            }

            updatedEllipse = default;
            return false;
        }

        private static bool TrySetLinePropertyPaneValue(this Line line, ClientPropertyPaneValue value, out Line updatedLine)
        {
            switch (value.Name)
            {
                case "x1":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p1: line.P1.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "y1":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p1: line.P1.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "z1":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p1: line.P1.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "x2":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p2: line.P2.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "y2":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p2: line.P2.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "z2":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(p2: line.P2.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "t":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLine = line.Update(thickness: unitValue);
                            return true;
                        }
                        break;
                    }
            }

            updatedLine = default;
            return false;
        }

        private static bool TrySetLocationPropertyPaneValue(this Location loc, ClientPropertyPaneValue value, out Location updatedLocation)
        {
            switch (value.Name)
            {
                case "x":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLocation = loc.Update(point: loc.Point.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "y":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLocation = loc.Update(point: loc.Point.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "z":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedLocation = loc.Update(point: loc.Point.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
            }

            updatedLocation = default;
            return false;
        }

        private static bool TrySetTextPropertyPaneValue(this Text text, ClientPropertyPaneValue value, out Text updatedText)
        {
            switch (value.Name)
            {
                case "v":
                    {
                        updatedText = text.Update(value: value.Value);
                        return true;
                    }
                case "x":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(location: text.Location.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "y":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(location: text.Location.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "z":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(location: text.Location.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
                case "h":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(height: unitValue);
                            return true;
                        }
                        break;
                    }
                case "r":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(rotation: unitValue);
                            return true;
                        }
                        break;
                    }
                case "nx":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(normal: text.Normal.WithX(unitValue));
                            return true;
                        }
                        break;
                    }
                case "ny":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(normal: text.Normal.WithY(unitValue));
                            return true;
                        }
                        break;
                    }
                case "nz":
                    {
                        if (DrawingSettings.TryParseUnits(value.Value, out var unitValue))
                        {
                            updatedText = text.Update(normal: text.Normal.WithZ(unitValue));
                            return true;
                        }
                        break;
                    }
            }

            updatedText = default;
            return false;
        }

        private static Point WithX(this Point point, double x)
        {
            return new Point(x, point.Y, point.Z);
        }

        private static Point WithY(this Point point, double y)
        {
            return new Point(point.X, y, point.Z);
        }

        private static Point WithZ(this Point point, double z)
        {
            return new Point(point.X, point.Y, z);
        }

        private static Vector WithX(this Vector vector, double x)
        {
            return new Vector(x, vector.Y, vector.Z);
        }

        private static Vector WithY(this Vector vector, double y)
        {
            return new Vector(vector.X, y, vector.Z);
        }

        private static Vector WithZ(this Vector vector, double z)
        {
            return new Vector(vector.X, vector.Y, z);
        }
    }
}
