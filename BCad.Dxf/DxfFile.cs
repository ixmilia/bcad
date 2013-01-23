﻿using System;
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
        public DxfTablesSection TablesSection { get; private set; }
        public DxfEntitiesSection EntitiesSection { get; private set; }

        internal IEnumerable<DxfSection> Sections
        {
            get
            {
                yield return this.HeaderSection;
                yield return this.TablesSection;
                yield return this.EntitiesSection;
            }
        }

        public DxfFile()
        {
            this.HeaderSection = new DxfHeaderSection();
            this.TablesSection = new DxfTablesSection();
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
                        case DxfSectionType.Tables:
                            file.TablesSection = (DxfTablesSection)section;
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
            writer.Open();

            // write sections
            foreach (var section in Sections)
            {
                var pairs = section.ValuePairs.ToList();
                if (pairs.Count == 0)
                    continue;
                writer.WriteCodeValuePair(new DxfCodePair(0, DxfSection.SectionText));
                writer.WriteCodeValuePair(new DxfCodePair(2, section.Type.ToSectionName()));
                writer.WriteCodeValuePairs(pairs);
                writer.WriteCodeValuePair(new DxfCodePair(0, DxfSection.EndSectionText));
            }

            writer.Close();
        }
    }
}
