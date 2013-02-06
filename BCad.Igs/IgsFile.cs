using System;
using System.IO;

namespace BCad.Igs
{
    public class IgsFile
    {
        internal const int MaxDataLength = 72;

        public char FieldDelimiter { get; set; }
        public char RecordDelimiter { get; set; }
        public string Identification { get; set; }
        public string FullFileName { get; set; }
        public string SystemIdentifier { get; set; }
        public string SystemVersion { get; set; }
        public int IntegerSize { get; set; }
        public int SingleSize { get; set; }
        public int DecimalDigits { get; set; }
        public int DoubleMagnitude { get; set; }
        public int DoublePrecision { get; set; }
        public string Identifier { get; set; }
        public double ModelSpaceScale { get; set; }
        public IgsUnits ModelUnits { get; set; }
        public string CustomModelUnits { get; set; }
        public int MaxLineWeightGraduations { get; set; }
        public double MaxLineWeight { get; set; }
        public DateTime TimeStamp { get; set; }
        public double MinimumResolution { get; set; }
        public double MaxCoordinateValue { get; set; }
        public string Author { get; set; }
        public string Organization { get; set; }
        public IegsVersion IegsVersion { get; set; }
        public IgsDraftingStandard DraftingStandard { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string ApplicationProtocol { get; set; }

        public IgsFile()
        {
            FieldDelimiter = ',';
            RecordDelimiter = ';';
            IntegerSize = 32;
            SingleSize = 8;
            DecimalDigits = 23;
            DoubleMagnitude = 11;
            DoublePrecision = 52;
            ModelSpaceScale = 1.0;
            ModelUnits = IgsUnits.Inches;
            MaxLineWeight = 1.0;
            TimeStamp = DateTime.Now;
            MinimumResolution = 1.0e-10;
            IegsVersion = Igs.IegsVersion.v5_3;
            DraftingStandard = IgsDraftingStandard.None;
        }

        public void Save(Stream stream)
        {
            var writer = new StreamWriter(stream);

            //// write start section
            //foreach (var section in new IgsSection[] { startSection, globalSection })
            //{
            //    int line = 1;
            //    foreach (var data in section.GetData())
            //    {
            //        writer.WriteLine(string.Format("{0,72}{1,1}{2,7}", data, SectionTypeChar(section.SectionType), line));
            //        line++;
            //    }
            //}

            writer.Flush();
        }

        public static IgsFile Load(Stream stream)
        {
            return IgsFileReader.Load(stream);
        }
    }
}
