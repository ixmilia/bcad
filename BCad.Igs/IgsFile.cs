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
    }
}
