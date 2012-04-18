using System;
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
        private DxfHeaderSection headerSection = new DxfHeaderSection();

        public List<DxfLayer> Layers { get { return tablesSection.Layers; } }
        public List<DxfEntity> Entities { get { return entitiesSection.Entities; } }

        internal IEnumerable<DxfSection> Sections
        {
            get
            {
                yield return headerSection;
                yield return tablesSection;
                yield return entitiesSection;
            }
        }

        public IEnumerable<DxfTable> Tables
        {
            get { return tablesSection.Tables; }
        }

        public string CurrentLayer
        {
            get { return headerSection.CurrentLayer; }
            set { headerSection.CurrentLayer = value; }
        }

        public DxfFile()
        {
        }

        public static DxfFile Open(string path)
        {
            return Open(new FileStream(path, FileMode.Open));
        }

        public static DxfFile Open(Stream stream)
        {
            var file = new DxfFile();
            var reader = new DxfReader(stream);
            foreach (var sec in reader.Sections)
            {
                if (sec is DxfTablesSection)
                    file.tablesSection = (DxfTablesSection)sec;
                else if (sec is DxfEntitiesSection)
                    file.entitiesSection = (DxfEntitiesSection)sec;
            }

            return file;
        }

        public static DxfFile Parse(string[] lines)
        {
            throw new NotImplementedException();
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
