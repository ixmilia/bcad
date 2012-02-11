using System.Collections.Generic;
using System.IO;
using BCad.Dxf.Entities;
using BCad.Dxf.Sections;
using BCad.Dxf.Tables;

namespace BCad.Dxf
{
    public class DxfFile
    {
        public const string BinarySentinel = "AutoCAD Binary DXF";
        public const string EofText = "EOF";

        private DxfTablesSection tablesSection = new DxfTablesSection();
        private DxfEntitiesSection entitiesSection = new DxfEntitiesSection();

        public List<DxfLayer> Layers { get { return tablesSection.Layers; } }
        public List<DxfEntity> Entities { get { return entitiesSection.Entities; } }

        public IEnumerable<DxfSection> Sections
        {
            get
            {
                yield return tablesSection;
                yield return entitiesSection;
            }
        }

        public IEnumerable<DxfTable> Tables
        {
            get { return tablesSection.Tables; }
        }

        public DxfFile()
        {
        }

        public DxfFile(string filename)
        {
            Open(filename);
        }

        public DxfFile(Stream stream)
        {
            Open(stream);
        }

        private void Open(string filename)
        {
            var reader = new DxfReader(filename);
            FromReader(reader);
        }

        private void Open(Stream stream)
        {
            var reader = new DxfReader(stream);
            FromReader(reader);
        }

        private void FromReader(DxfReader reader)
        {
            foreach (var sec in reader.Sections)
            {
                if (sec is DxfTablesSection)
                    tablesSection = (DxfTablesSection)sec;
                else if (sec is DxfEntitiesSection)
                    entitiesSection = (DxfEntitiesSection)sec;
            }
        }

        public void Save(string filename, bool asText = true)
        {
            var stream = new FileStream(filename, FileMode.OpenOrCreate); 
            WriteStream(stream, asText);
        }

        public void Save(Stream stream, bool asText = true)
        {
            WriteStream(stream, asText);
        }

        private void WriteStream(Stream stream, bool asText)
        {
            var writer = new DxfWriter(stream, asText);
            writer.Write(this);
            writer.Close();
        }
    }
}
