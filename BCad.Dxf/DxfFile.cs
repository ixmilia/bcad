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

        public DxfHeaderSection HeaderSection { get; private set; }
        public DxfEntitiesSection EntitiesSection { get; private set; }

        private DxfTablesSection tablesSection = new DxfTablesSection();

        public List<DxfLayer> Layers { get { return tablesSection.Layers; } }
        public List<DxfViewPort> ViewPorts { get { return tablesSection.ViewPorts; } }

        internal IEnumerable<DxfSection> Sections
        {
            get
            {
                yield return this.HeaderSection;
                yield return tablesSection;
                yield return this.EntitiesSection;
            }
        }

        public IEnumerable<DxfTable> Tables
        {
            get { return tablesSection.Tables; }
        }

        public DxfFile()
        {
            this.HeaderSection = new DxfHeaderSection();
            this.EntitiesSection = new DxfEntitiesSection();
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
            var buffer = new DxfCodePairBufferReader(reader.ReadCodePairs());
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (DxfCodePair.IsSectionStart(pair))
                {
                    buffer.Advance(); // swallow (0, SECTION) pair
                    var section = DxfSection.FromBuffer(buffer);
                    switch (section.Type)
                    {
                        case DxfSectionType.Entities:
                            file.EntitiesSection = (DxfEntitiesSection)section;
                            break;
                        case DxfSectionType.Header:
                            file.HeaderSection = (DxfHeaderSection)section;
                            break;
                    }
                }
                else if (DxfCodePair.IsEof(pair))
                {
                    // swallow and quit
                    buffer.Advance();
                    break;
                }
            }

            Debug.Assert(!buffer.ItemsRemain);

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
