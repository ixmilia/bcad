using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Rpc
{
    public static class PropertyExtensions
    {
        private static IEnumerable<string> GetAllLayerNames(this Drawing drawing) => drawing.GetLayers().Select(l => l.Name).OrderBy(s => s);

        private static string ToPropertyColorString(this CadColor? color) => color?.ToRGBString();

        private static ClientPropertyPaneValue GetEntityLayerValue(Drawing drawing, string entityLayer, bool isUnrepresentable = false)
        {
            return ClientPropertyPaneValue.Create("layer", "Layer", entityLayer, (drawing, entity, newLayerName) =>
            {
                var containingLayer = drawing.ContainingLayer(entity);
                if (containingLayer.Name == newLayerName)
                {
                    // no change
                    return Tuple.Create<Drawing, Entity>(drawing, null);
                }

                var destinationLayer = drawing.Layers.GetValue(newLayerName);
                var updatedContainingLayer = containingLayer.Remove(entity);
                var updatedDestinationLayer = destinationLayer.Add(entity);
                var updatedDrawing = drawing
                    .Remove(containingLayer)
                    .Remove(destinationLayer)
                    .Add(updatedContainingLayer)
                    .Add(updatedDestinationLayer);
                return Tuple.Create<Drawing, Entity>(updatedDrawing, null);
            }, drawing.GetAllLayerNames(), isUnrepresentable: isUnrepresentable);
        }

        private static ClientPropertyPaneValue GetEntityColorValue(CadColor? currentColor, bool isUnrepresentable = false)
        {
            return ClientPropertyPaneValue.CreateForEntity<Entity>("color", "Color", currentColor.ToPropertyColorString(), (entity, colorString) =>
            {
                CadColor? newColor = colorString == null ? null : CadColor.Parse(colorString);
                if (entity.Color == newColor)
                {
                    // no change
                    return null;
                }

                var updatedEntity = entity.WithColor(newColor);
                return updatedEntity;
            }, isUnrepresentable: isUnrepresentable);
        }

        public static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValues(this IWorkspace workspace)
        {
            return workspace.Drawing.GetPropertyPaneValues(workspace.SelectedEntities);
        }

        public static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValues(this Drawing drawing, IEnumerable<Entity> entities)
        {
            var selectedEntities = entities.ToList();
            if (selectedEntities.Count == 0)
            {
                return Enumerable.Empty<ClientPropertyPaneValue>();
            }
            else if (selectedEntities.Count == 1)
            {
                return drawing.GetPropertyPaneValuesForSingleEntity(selectedEntities.Single());
            }
            else
            {
                return drawing.GetPropertyPaneValuesForMultipleEntities(selectedEntities);
            }
        }

        private static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValuesForMultipleEntities(this Drawing drawing, IEnumerable<Entity> entities)
        {
            var distinctLayerNames = entities.Select(e => drawing.ContainingLayer(e)).Select(l => l.Name).Distinct().ToList();
            var selectedLayerName = distinctLayerNames.Count == 1
                ? distinctLayerNames.Single()
                : null;

            yield return GetEntityLayerValue(drawing, selectedLayerName, isUnrepresentable: distinctLayerNames.Count > 1);

            var distinctColors = entities.Select(e => e.Color).Distinct().ToList();
            var selectedColor = distinctColors.Count == 1
                ? distinctColors.Single()
                : null;

            yield return GetEntityColorValue(selectedColor, distinctColors.Count > 1);
        }

        private static IEnumerable<ClientPropertyPaneValue> GetPropertyPaneValuesForSingleEntity(this Drawing drawing, Entity entity)
        {
            var layer = drawing.ContainingLayer(entity);

            var general = new[]
            {
                GetEntityLayerValue(drawing, layer.Name),
                GetEntityColorValue(entity.Color),
            };

            var specific = entity.MapEntity<ClientPropertyPaneValue[]>(
                aggregate => new ClientPropertyPaneValue[0],
                arc => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("cx", "Center X", drawing.FormatUnits(arc.Center.X), (arc, value) => arc.Update(center: arc.Center.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("cy", "Y", drawing.FormatUnits(arc.Center.Y), (arc, value) => arc.Update(center: arc.Center.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("cz", "Z", drawing.FormatUnits(arc.Center.Z), (arc, value) => arc.Update(center: arc.Center.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("r", "Radius", drawing.FormatUnits(arc.Radius), (arc, value) => arc.Update(radius: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("sa", "Start Angle", drawing.FormatAngle(arc.StartAngle), (arc, value) => arc.Update(startAngle: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("ea", "End Angle", drawing.FormatAngle(arc.EndAngle), (arc, value) => arc.Update(endAngle: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("nx", "Normal X", drawing.FormatUnits(arc.Normal.X), (arc, value) => arc.Update(normal: arc.Normal.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("ny", "Y", drawing.FormatUnits(arc.Normal.Y), (arc, value) => arc.Update(normal: arc.Normal.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("nz", "Z", drawing.FormatUnits(arc.Normal.Z), (arc, value) => arc.Update(normal: arc.Normal.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Arc>("t", "Thickness", drawing.FormatUnits(arc.Thickness), (arc, value) => arc.Update(thickness: value)),
                    ClientPropertyPaneValue.CreateReadOnly("Start Point", drawing.FormatPoint(arc.EndPoint1)),
                    ClientPropertyPaneValue.CreateReadOnly("End Point", drawing.FormatPoint(arc.EndPoint2)),
                    ClientPropertyPaneValue.CreateReadOnly("Total Angle", drawing.FormatAngle(arc.TotalAngle())),
                    ClientPropertyPaneValue.CreateReadOnly("Arc Length", drawing.FormatUnits(arc.ArcLength())),
                },
                circle => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("cx", "Center X", drawing.FormatUnits(circle.Center.X), (circle, value) => circle.Update(center: circle.Center.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("cy", "Y", drawing.FormatUnits(circle.Center.Y), (circle, value) => circle.Update(center: circle.Center.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("cz", "Z", drawing.FormatUnits(circle.Center.Z), (circle, value) => circle.Update(center: circle.Center.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("r", "Radius", drawing.FormatUnits(circle.Radius), (circle, value) => circle.Update(radius: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("nx", "Normal X", drawing.FormatUnits(circle.Normal.X), (circle, value) => circle.Update(normal: circle.Normal.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("ny", "Y", drawing.FormatUnits(circle.Normal.Y), (circle, value) => circle.Update(normal: circle.Normal.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("nz", "Z", drawing.FormatUnits(circle.Normal.Z), (circle, value) => circle.Update(normal: circle.Normal.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Circle>("t", "Thickness", drawing.FormatUnits(circle.Thickness), (circle, value) => circle.Update(thickness: value)),
                    ClientPropertyPaneValue.CreateReadOnly("Area", drawing.FormatScalar(circle.Area())),
                    ClientPropertyPaneValue.CreateReadOnly("Diameter", drawing.FormatUnits(circle.Diameter())),
                    ClientPropertyPaneValue.CreateReadOnly("Circumference", drawing.FormatUnits(circle.Circumference())),
                },
                ellipse => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("cx", "Center X", drawing.FormatUnits(ellipse.Center.X), (ellipse, value) => ellipse.Update(center: ellipse.Center.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("cy", "Y", drawing.FormatUnits(ellipse.Center.Y), (ellipse, value) => ellipse.Update(center: ellipse.Center.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("cz", "Z", drawing.FormatUnits(ellipse.Center.Z), (ellipse, value) => ellipse.Update(center: ellipse.Center.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("mx", "Major Axis X", drawing.FormatUnits(ellipse.MajorAxis.X), (ellipse, value) => ellipse.Update(majorAxis: ellipse.MajorAxis.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("my", "Y", drawing.FormatUnits(ellipse.MajorAxis.Y), (ellipse, value) => ellipse.Update(majorAxis: ellipse.MajorAxis.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("mz", "Z", drawing.FormatUnits(ellipse.MajorAxis.Z), (ellipse, value) => ellipse.Update(majorAxis: ellipse.MajorAxis.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("mr", "Minor Axis Ratio", drawing.FormatScalar(ellipse.MinorAxisRatio), (ellipse, value) => ellipse.Update(minorAxisRatio: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("sa", "Start Angle", drawing.FormatAngle(ellipse.StartAngle), (ellipse, value) => ellipse.Update(startAngle: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("ea", "End Angle", drawing.FormatAngle(ellipse.EndAngle), (ellipse, value) => ellipse.Update(endAngle: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("nx", "Normal X", drawing.FormatUnits(ellipse.Normal.X), (ellipse, value) => ellipse.Update(normal: ellipse.Normal.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("ny", "Y", drawing.FormatUnits(ellipse.Normal.Y), (ellipse, value) => ellipse.Update(normal: ellipse.Normal.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("nz", "Z", drawing.FormatUnits(ellipse.Normal.Z), (ellipse, value) => ellipse.Update(normal: ellipse.Normal.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Ellipse>("t", "Thickness", drawing.FormatUnits(ellipse.Thickness), (ellipse, value) => ellipse.Update(thickness: value)),
                },
                image => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("x", "Location X", drawing.FormatUnits(image.Location.X), (image, value) => image.Update(location: image.Location.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("y", "Y", drawing.FormatUnits(image.Location.Y), (image, value) => image.Update(location: image.Location.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("z", "Z", drawing.FormatUnits(image.Location.Z), (image, value) => image.Update(location: image.Location.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntity<Image>("p", "Path", image.Path, (image, value) => image.Update(path: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("w", "Width", drawing.FormatUnits(image.Width), (image, value) => image.Update(width: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("h", "Height", drawing.FormatUnits(image.Height), (image, value) => image.Update(height: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Image>("r", "Rotation", drawing.FormatAngle(image.Rotation), (image, value) => image.Update(rotation: value)),
                },
                line => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("x1", "Start X", drawing.FormatUnits(line.P1.X), (line, value) => line.Update(p1: line.P1.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("y1", "Y", drawing.FormatUnits(line.P1.Y), (line, value) => line.Update(p1: line.P1.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("z1", "Z", drawing.FormatUnits(line.P1.Z), (line, value) => line.Update(p1: line.P1.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("x2", "End X", drawing.FormatUnits(line.P2.X), (line, value) => line.Update(p2: line.P2.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("y2", "Y", drawing.FormatUnits(line.P2.Y), (line, value) => line.Update(p2: line.P2.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("z2", "Z", drawing.FormatUnits(line.P2.Z), (line, value) => line.Update(p2: line.P2.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Line>("t", "Thickness", drawing.FormatUnits(line.Thickness), (line, value) => line.Update(thickness: value)),
                    ClientPropertyPaneValue.CreateReadOnly("Length", drawing.FormatUnits(line.Length())),
                    ClientPropertyPaneValue.CreateReadOnly("Delta", drawing.FormatVector(line.Delta())),
                    ClientPropertyPaneValue.CreateReadOnly("Angle", drawing.FormatAngle(line.AngleInDegrees())),
                },
                location => new[]
                {
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Location>("x", "Location X", drawing.FormatUnits(location.Point.X), (location, value) => location.Update(point: location.Point.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Location>("y", "Y", drawing.FormatUnits(location.Point.Y), (location, value) => location.Update(point: location.Point.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Location>("z", "Z", drawing.FormatUnits(location.Point.Z), (location, value) => location.Update(point: location.Point.WithZ(value))),
                },
                polyline => new ClientPropertyPaneValue[0],
                spline => new ClientPropertyPaneValue[0],
                text => new[]
                {
                    ClientPropertyPaneValue.CreateForEntity<Text>("v", "Value", text.Value, (text, value) => text.Update(value: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("x", "Location X", drawing.FormatUnits(text.Location.X), (text, value) => text.Update(location: text.Location.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("y", "Y", drawing.FormatUnits(text.Location.Y), (text, value) => text.Update(location: text.Location.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("z", "Z", drawing.FormatUnits(text.Location.Z), (text, value) => text.Update(location: text.Location.WithZ(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("h", "Height", drawing.FormatUnits(text.Height), (text, value) => text.Update(height: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("r", "Rotation", drawing.FormatAngle(text.Rotation), (text, value) => text.Update(rotation: value)),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("nx", "Normal X", drawing.FormatUnits(text.Normal.X), (text, value) => text.Update(normal: text.Normal.WithX(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("ny", "Y", drawing.FormatUnits(text.Normal.Y), (text, value) => text.Update(normal: text.Normal.WithY(value))),
                    ClientPropertyPaneValue.CreateForEntityWithUnits<Text>("nz", "Z", drawing.FormatUnits(text.Normal.Z), (text, value) => text.Update(normal: text.Normal.WithZ(value))),
                }
            ); ;

            return general.Concat(specific);
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
