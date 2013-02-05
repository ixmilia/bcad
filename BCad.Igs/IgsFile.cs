using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Igs
{
    public class IgsFile
    {
        private IgsStartSection startSection;
        private IgsGlobalSection globalSection;

        public string StartData
        {
            get { return startSection.Data; }
            set { startSection.Data = value; }
        }

        public IgsFile()
        {
            startSection = new IgsStartSection();
            globalSection = new IgsGlobalSection();
        }

        public void Save(Stream stream)
        {
            var writer = new StreamWriter(stream);

            // write start section
            foreach (var section in new IgsSection[] { startSection, globalSection })
            {
                int line = 1;
                foreach (var data in section.GetData())
                {
                    writer.WriteLine(string.Format("{0,72}{1,1}{2,7}", data, SectionTypeChar(section.SectionType), line));
                    line++;
                }
            }

            writer.Flush();
        }

        public static IgsFile Load(Stream stream)
        {
            var file = new IgsFile();
            var lines = new StreamReader(stream).ReadToEnd().Split("\n".ToCharArray()).Select(s => s.TrimEnd());
            var currentType = IgsSectionType.None;
            int currentLine = 0;
            foreach (var line in lines)
            {
                if (line.Length != 80)
                    throw new IgsException("Expected line length of 80 characters.");
                var data = line.Substring(0, IgsSection.MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[IgsSection.MaxDataLength]);
                var lineNumber = int.Parse(line.Substring(IgsSection.MaxDataLength + 1).TrimStart());

                if (lineNumber == 1)
                {
                    // new section
                    if ((int)currentType + 1 != (int)sectionType)
                        throw new IgsException("Section unexpected at this time " + sectionType);
                }
                else if (currentLine + 1 != lineNumber)
                {
                    // out-of-order line
                    throw new IgsException("Expected line number " + lineNumber);
                }
                else
                {
                    currentLine = lineNumber;
                }
            }

            return file;
        }

        private static char SectionTypeChar(IgsSectionType type)
        {
            switch (type)
            {
                case IgsSectionType.Start: return 'S';
                case IgsSectionType.Global: return 'G';
                case IgsSectionType.DirectoryEntry: return 'D';
                case IgsSectionType.ParameterData: return 'P';
                case IgsSectionType.Terminate: return 'T';
                default:
                    throw new IgsException("Unexpected section type " + type);
            }
        }

        private static IgsSectionType SectionTypeFromCharacter(char c)
        {
            switch (c)
            {
                case 'S': return IgsSectionType.Start;
                case 'G': return IgsSectionType.Global;
                case 'D': return IgsSectionType.DirectoryEntry;
                case 'P': return IgsSectionType.ParameterData;
                case 'T': return IgsSectionType.Terminate;
                default:
                    throw new IgsException("Invalid section type " + c);
            }
        }
    }
}
