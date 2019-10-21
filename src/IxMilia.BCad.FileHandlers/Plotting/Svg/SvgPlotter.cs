// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Xml;
using System.Xml.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Plotting.Svg
{
    internal class SvgPlotter : PlotterBase
    {
        internal static XNamespace Xmlns = "http://www.w3.org/2000/svg";

        public SvgPlotterViewModel ViewModel { get; }

        public SvgPlotter(SvgPlotterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override void Plot(IWorkspace workspace)
        {
            var autoColor = CadColor.Black;
            var root = new XElement(Xmlns + "svg",
                //new XAttribute("width", string.Format("{0}in", 6)),
                //new XAttribute("height", string.Format("{0}in", 6)),
                new XAttribute("viewBox", string.Format("{0} {1} {2} {3}", 0, 0, ViewModel.Width, ViewModel.Height)),
                new XAttribute("version", "1.1"));
            var projectedEntities = ProjectionHelper.ProjectTo2D(
                workspace.Drawing,
                ViewModel.ViewPort,
                ViewModel.Width,
                ViewModel.Height,
                Display.ProjectionStyle.OriginTopLeft);
            foreach (var groupedEntity in projectedEntities.GroupBy(p => p.OriginalLayer).OrderBy(x => x.Key.Name))
            {
                var layer = groupedEntity.Key;
                root.Add(new XComment(string.Format(" layer '{0}' ", layer.Name)));
                var g = new XElement(Xmlns + "g",
                    new XAttribute("stroke", (layer.Color ?? autoColor).ToRGBString()),
                    new XAttribute("fill", (layer.Color ?? autoColor).ToRGBString()));
                foreach (var entity in groupedEntity)
                {
                    var elem = entity.ToXElement();
                    if (elem != null)
                        g.Add(elem);
                }

                root.Add(g);
            }

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };

            using (var writer = XmlWriter.Create(ViewModel.Stream, settings))
            {
                var doc = new XDocument(
                    new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
                    root);
                doc.WriteTo(writer);
                writer.Flush();
            }
        }
    }
}
