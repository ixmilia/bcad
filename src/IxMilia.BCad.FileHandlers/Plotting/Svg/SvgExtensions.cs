using System;
using System.Linq;
using System.Xml.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Plotting.Svg
{
    internal static class SvgExtensions
    {
        public static XElement ToXElement(this ProjectedEntity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return ToXElement((ProjectedLine)entity);
                case EntityKind.Arc:
                    return ToXElement((ProjectedArc)entity);
                case EntityKind.Circle:
                    return ToXElement((ProjectedCircle)entity);
                case EntityKind.Text:
                    return ToXElement((ProjectedText)entity);
                case EntityKind.Aggregate:
                    return ToXElement((ProjectedAggregate)entity);
                default:
                    return null;
            }
        }

        private static XElement ToXElement(ProjectedLine line)
        {
            var scale = (line.P2 - line.P1).Length / (line.OriginalLine.P2 - line.OriginalLine.P1).Length;
            var xml = new XElement(SvgPlotter.Xmlns + "line",
                new XAttribute("x1", line.P1.X),
                new XAttribute("y1", line.P1.Y),
                new XAttribute("x2", line.P2.X),
                new XAttribute("y2", line.P2.Y));
            AddStrokeIfNotDefault(xml, line.OriginalLine.Color);
            AddStrokeWidth(xml, PlotterBase.ApplyScaleToThickness(line.OriginalLine.Thickness, scale));
            return xml;
        }

        private static XElement ToXElement(ProjectedText text)
        {
            var xml = new XElement(SvgPlotter.Xmlns + "text",
                new XAttribute("x", text.Location.X),
                new XAttribute("y", text.Location.Y),
                new XAttribute("font-size", string.Format("{0}px", text.Height)),
                text.OriginalText.Value);
            AddRotationTransform(xml, text.Rotation, text.Location);
            AddStrokeIfNotDefault(xml, text.OriginalText.Color);
            AddFillIfNotDefault(xml, text.OriginalText.Color);
            return xml;
        }

        private static XElement ToXElement(ProjectedCircle circle)
        {
            var xml = new XElement(SvgPlotter.Xmlns + "ellipse",
                new XAttribute("cx", circle.Center.X),
                new XAttribute("cy", circle.Center.Y),
                new XAttribute("rx", circle.RadiusX),
                new XAttribute("ry", circle.RadiusY),
                new XAttribute("fill-opacity", 0));
            AddRotationTransform(xml, circle.Rotation, circle.Center);
            AddStrokeIfNotDefault(xml, circle.OriginalCircle.Color);
            return xml;
        }

        private static XElement ToXElement(ProjectedAggregate aggregate)
        {
            var group = new XElement(SvgPlotter.Xmlns + "g",
                aggregate.Children.Select(c => ToXElement(c)));
            AddTranslateTransform(group, (Vector)aggregate.Location);
            AddStrokeIfNotDefault(group, aggregate.Original.Color);
            return group;
        }

        private static XElement ToXElement(ProjectedArc arc)
        {
            var pathData = string.Format("M {0} {1} a {2} {3} {4} {5} {6} {7} {8}",
                arc.StartPoint.X,
                arc.StartPoint.Y,
                arc.RadiusX,
                arc.RadiusY,
                0, // x axis rotation
                0, // flag
                0, // flag
                arc.EndPoint.X - arc.StartPoint.X,
                arc.EndPoint.Y - arc.StartPoint.Y);
            var lineData = string.Format("M {0} {1} L {2} {3}",
                arc.StartPoint.X,
                arc.StartPoint.Y,
                arc.EndPoint.X,
                arc.EndPoint.Y);
            var xml = new XElement(SvgPlotter.Xmlns + "path",
                new XAttribute("d", pathData),
                new XAttribute("fill-opacity", 0));
            AddRotationTransform(xml, arc.Rotation, arc.Center);
            AddStrokeIfNotDefault(xml, arc.OriginalArc.Color);
            return xml;
        }

        private static void AddRotationTransform(XElement xml, double angle, Point location)
        {
            if (!MathHelper.CloseTo(0, angle) && !MathHelper.CloseTo(360, angle))
            {
                var rotateText = string.Format("rotate({0} {1} {2})", angle * -1.0, location.X, location.Y);
                AddTransform(xml, rotateText);
            }
        }

        private static void AddTranslateTransform(XElement xml, Vector offset)
        {
            var translateText = string.Format("translate({0} {1})", offset.X, offset.Y);
            AddTransform(xml, translateText);
        }

        private static void AddTransform(XElement xml, string transform)
        {
            var attribute = xml.Attribute("transform");
            if (attribute == null)
            {
                // add new attribute
                xml.Add(new XAttribute("transform", transform));
            }
            else
            {
                // append a space and the transformation
                attribute.Value += " " + transform;
            }
        }

        private static void AddStrokeIfNotDefault(XElement xml, CadColor? color)
        {
            if (color.HasValue)
            {
                var stroke = xml.Attribute("stroke");
                var colorString = color.Value.ToRGBString();
                if (stroke == null)
                {
                    // add new attribute
                    xml.Add(new XAttribute("stroke", colorString));
                }
                else
                {
                    // replace attribute
                    stroke.Value = colorString;
                }
            }
        }

        private static void AddStrokeWidth(XElement xml, double strokeWidth)
        {
            xml.Add(new XAttribute("stroke-width", $"{Math.Max(strokeWidth, 1.0)}px"));
        }

        private static void AddFillIfNotDefault(XElement xml, CadColor? color)
        {
            if (color.HasValue)
            {
                var stroke = xml.Attribute("fill");
                var colorString = color.Value.ToRGBString();
                if (stroke == null)
                {
                    // add new attribute
                    xml.Add(new XAttribute("fill", colorString));
                }
                else
                {
                    // replace attribute
                    stroke.Value = colorString;
                }
            }
        }
    }
}
