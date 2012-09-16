using System;
using System.Collections.Generic;
using System.IO;
using BCad.Dxf.Entities;
using BCad.Dxf.Sections;
using BCad.Dxf.Tables;
using System.Diagnostics;

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
        public List<DxfViewPort> ViewPorts { get { return tablesSection.ViewPorts; } }

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

        public static DxfFile Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load(stream);
            }
        }

        public static DxfFile Load(Stream stream)
        {
            var file = new DxfFile();
            var reader = new DxfReader(stream);
            foreach (var sec in reader.Sections)
            {
                if (sec is DxfTablesSection)
                    file.tablesSection = (DxfTablesSection)sec;
                else if (sec is DxfEntitiesSection)
                    file.entitiesSection = (DxfEntitiesSection)sec;
                else if (sec is DxfHeaderSection)
                    file.headerSection = (DxfHeaderSection)sec;
                else
                    Debug.Fail("Unknown section: " + sec.GetType().Name);
            }

            return file;
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
