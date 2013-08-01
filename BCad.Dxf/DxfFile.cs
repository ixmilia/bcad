using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BCad.Dxf.Entities;
using BCad.Dxf.Sections;
using BCad.Dxf.Tables;

namespace BCad.Dxf
{
    public class DxfFile
    {
        public const string BinarySentinel = "AutoCAD Binary DXF";
        public const string EofText = "EOF";

        public DxfHeaderSection HeaderSection { get; private set; }
        public DxfClassesSection ClassSection { get; private set; }
        public DxfTablesSection TablesSection { get; private set; }
        public DxfBlocksSection BlocksSection { get; private set; }
        public DxfEntitiesSection EntitiesSection { get; private set; }

        internal IEnumerable<DxfSection> Sections
        {
            get
            {
                yield return this.HeaderSection;
                yield return this.ClassSection;
                yield return this.TablesSection;
                yield return this.BlocksSection;
                yield return this.EntitiesSection;
            }
        }

        public DxfFile()
        {
            this.HeaderSection = new DxfHeaderSection();
            this.ClassSection = new DxfClassesSection();
            this.TablesSection = new DxfTablesSection();
            this.BlocksSection = new DxfBlocksSection();
            this.EntitiesSection = new DxfEntitiesSection();
        }

        // TODO: #if !SILVERLIGHT || !WIN_RT
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
                    if (section != null)
                    {
                        switch (section.Type)
                        {
                            case DxfSectionType.Blocks:
                                file.BlocksSection = (DxfBlocksSection)section;
                                break;
                            case DxfSectionType.Entities:
                                file.EntitiesSection = (DxfEntitiesSection)section;
                                break;
                            case DxfSectionType.Classes:
                                file.ClassSection = (DxfClassesSection)section;
                                break;
                            case DxfSectionType.Header:
                                file.HeaderSection = (DxfHeaderSection)section;
                                break;
                            case DxfSectionType.Tables:
                                file.TablesSection = (DxfTablesSection)section;
                                break;
                        }
                    }
                }
                else if (DxfCodePair.IsEof(pair))
                {
                    // swallow and quit
                    buffer.Advance();
                    break;
                }
                else if (DxfCodePair.IsComment(pair))
                {
                    // swallow comments
                    buffer.Advance();
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
            writer.Open();

            // write sections
            foreach (var section in Sections)
            {
                foreach (var pair in section.GetValuePairs())
                    writer.WriteCodeValuePair(pair);
            }

            writer.Close();
        }
    }
}
