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

        public string StartData
        {
            get { return startSection.Data; }
            set { startSection.Data = value; }
        }

        public IgsFile()
        {
            startSection = new IgsStartSection();
        }

        public void Save(Stream stream)
        {
            var writer = new StreamWriter(stream);

            // write start section
            int line = 1;
            foreach (var data in startSection.GetData())
            {
                writer.WriteLine(string.Format("{0,72}{1,1}{2,7}", data, "S", line));
                line++;
            }

            writer.Flush();
        }

        public static IgsFile Load(Stream stream)
        {
            var file = new IgsFile();
            var lines = new StreamReader(stream).ReadToEnd().Split("\n".ToCharArray()).Select(s => s.TrimEnd());
            foreach (var line in lines)
            {
                if (line.Length != 80)
                    throw new IgsException("Expected line length of 80 characters.");
                var data = line.Substring(0, IgsSection.MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[IgsSection.MaxDataLength]);
            }

            return file;
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
