using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad.Entities;

namespace BCad.FilePlotters
{
    [ExportFilePlotter(DisplayName, FileExtension)]
    internal class PdfFilePlotter : IFilePlotter
    {
        public const string DisplayName = "PDF Files (" + FileExtension + ")";
        public const string FileExtension = ".pdf";

        private StringBuilder _builder = new StringBuilder();
        private List<int> _objectOffsets = new List<int>();
        private CadColor _lastColor = CadColor.Black;

        private static CadColor AutoColor = CadColor.Black;

        public void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream)
        {
            _builder.Clear();
            _objectOffsets.Clear();

            OutputHeader(width, height);
            var body = new StringBuilder();
            body.AppendLine("/DeviceRGB CS");
            // set line width with: body.AppendLine("0.5 w");  // 1 == 1/72"
            SetColor(body, CadColor.Black, doStroke: false);
            foreach (var group in entities.GroupBy(e => e.OriginalLayer).OrderBy(l => l.Key.Name))
            {
                var layer = group.Key;
                body.AppendLine();
                body.AppendLine($"% layer '{layer.Name}'");
                foreach (var entity in group)
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (ProjectedLine)entity;
                            SetColor(body, line.OriginalLine.Color ?? line.OriginalLayer.Color ?? AutoColor);
                            body.AppendLine($"{line.P1.X:f} {line.P1.Y:f} m");
                            body.AppendLine($"{line.P2.X:f} {line.P2.Y:f} l");
                            break;
                        default:
                            // TODO:
                            break;
                    }
                }
            }
            body.AppendLine("S"); // final stroke

            AddOffset();
            _builder.AppendLine($"4 0 obj <</Length {body.Length}>>");
            _builder.AppendLine("stream");
            _builder.Append(body.ToString());
            _builder.AppendLine(); // stream is required to end in \r\n
            _builder.AppendLine("endstream");
            _builder.AppendLine("endobj");

            _builder.AppendLine();
            _builder.AppendLine("xref");
            var xrefLoc = CurrentOffset;
            var xrefCount = _objectOffsets.Count + 1; // + 1 to account for the required zero-id free object
            _builder.AppendLine($"0 {xrefCount}");
            _builder.AppendLine($"0000000000 {ushort.MaxValue} f"); // said required zero-id free object
            for (int i = 0; i < _objectOffsets.Count; i++)
            {
                _builder.AppendLine($"{_objectOffsets[i].ToString().PadLeft(10, '0')} {(0).ToString().PadLeft(5, '0')} n");
            }

            _builder.AppendLine($"trailer <</Size {xrefCount} /Root 1 0 R>>");
            _builder.AppendLine("startxref");
            _builder.AppendLine(xrefLoc.ToString());
            _builder.AppendLine("%%EOF");

            var writer = new StreamWriter(stream);
            writer.Write(_builder.ToString());
            writer.Flush();
            writer.Close();
        }

        private void SetColor(StringBuilder body, CadColor color, bool doStroke = true)
        {
            if (color != _lastColor)
            {
                if (doStroke)
                {
                    body.AppendLine("S");
                }

                body.AppendLine($"{color.R / 255.0} {color.G / 255.0} {color.B / 255.0} SC");
                _lastColor = color;
            }
        }

        private int CurrentOffset => _builder.Length;

        private void AddOffset()
        {
            _objectOffsets.Add(CurrentOffset);
        }

        private void OutputHeader(double width, double height)
        {
            _builder.AppendLine("%PDF-1.6");

            AddOffset();
            _builder.AppendLine("1 0 obj <</Type /Catalog /Pages 2 0 R>>");
            _builder.AppendLine("endobj");

            AddOffset();
            _builder.AppendLine("2 0 obj <</Type /Pages /Kids [3 0 R] /Count 1>>");
            _builder.AppendLine("endobj");

            AddOffset();
            _builder.AppendLine($"3 0 obj <</Type /Page /Parent 2 0 R /Contents 4 0 R /MediaBox [0 0 {width:f} {height:f}]>>");
            _builder.AppendLine("endobj");
        }
    }
}
