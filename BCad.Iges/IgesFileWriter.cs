using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iges.Directory;
using BCad.Iges.Parameter;

namespace BCad.Iges
{
    internal class IgesFileWriter
    {
        public void Write(IgesFile file, Stream stream)
        {
            var writer = new StreamWriter(stream);

            //// write start section
            //foreach (var section in new IgesSection[] { startSection, globalSection })
            //{
            //    int line = 1;
            //    foreach (var data in section.GetData())
            //    {
            //        writer.WriteLine(string.Format("{0,72}{1,1}{2,7}", data, SectionTypeChar(section.SectionType), line));
            //        line++;
            //    }
            //}

            // prepare entities
            var directoryLines = new List<string>();
            var parameterLines = new List<string>();
            int parameterLine = 0;
            int directoryLine = 0;

            foreach (var entity in file.Entities)
            {
                var directory = IgesDirectoryData.FromEntity(entity, parameterLine);
                var parameter = IgesParameterData.FromEntity(entity);

                directoryLines.Add(directory.ToString(directoryLine));
                parameterLines.Add(parameter.ToString(entity.Type, parameterLine));
            }

            writer.Flush();
        }
    }
}
