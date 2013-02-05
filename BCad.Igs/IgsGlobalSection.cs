using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Igs
{
    internal enum ModelUnits
    {
        Inches = 1,
        Millimeter = 2,
        Custom = 3,
        Feet = 4,
        Miles = 5,
        Meters = 6,
        Kilometers = 7,
        Mils = 8,
        Microns = 9,
        Centimeters = 10,
        MicroInches = 11
    }

    internal enum IegsVersion
    {
        v1_0 = 1,
        ANSI_1981 = 2,
        v2_0 = 3,
        v3_0 = 4,
        ANSI_1987 = 5,
        v4_0 = 6,
        ANSI_1989 = 7,
        v5_0 = 8,
        v5_1 = 9,
        USPRO5_2 = 10,
        v5_3 = 11
    }

    internal enum IgsDraftingStandard
    {
        None = 0,
        ISO = 1,
        AFNOR = 2,
        ANSI = 3,
        BSI = 4,
        CSA = 5,
        DIN = 6,
        JIS = 7
    }

    internal class IgsGlobalSection : IgsSection
    {
        private char parameterDelimiter = ',';
        private char recordDelimiter = ';';

        protected override IgsSectionType SectionType
        {
            get { return IgsSectionType.Global; }
        }

        public string ProductIdentification { get; set; }

        public string FullFileName { get; set; }

        public string ExportingSystem { get; set; }

        public string SystemVersion { get; set; }

        public int IntegerSize { get; set; }

        public int SinglePrecision { get; set; }

        public int SignificantDecimalDigits { get; set; }

        public int DoublePrecisionMagnitude { get; set; }

        public int DoublePrecisionSignificance { get; set; }

        public string Identifier { get; set; }

        public double ModelSpaceScale { get; set; }

        public ModelUnits ModelUnits { get; set; }

        public string CustomModelUnits { get; set; }

        public int MaxLineWeightGraduations { get; set; }

        public double MaxLineWidth { get; set; }

        public DateTime TimeStamp { get; set; }

        public double Epsilon { get; set; }

        public double ApproxLargestCoordinateValue { get; set; }

        public string Author { get; set; }

        public string Organization { get; set; }

        public IegsVersion Version { get; set; }

        public IgsDraftingStandard DraftingStandard { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ApplicationProtocol { get; set; }

        public IgsGlobalSection()
        {
            IntegerSize = 32;
            SinglePrecision = 8;
            SignificantDecimalDigits = 23;
            DoublePrecisionMagnitude = 11;
            DoublePrecisionSignificance = 52;
            ModelSpaceScale = 1.0;
            ModelUnits = Igs.ModelUnits.Inches;
            MaxLineWeightGraduations = 4;
            MaxLineWidth = 1.0;
            TimeStamp = DateTime.Now;
            Epsilon = 1.0e-10;
            Version = IegsVersion.v5_3;
            DraftingStandard = IgsDraftingStandard.None;
            ModifiedDate = DateTime.Now;
        }

        public override IEnumerable<string> GetData()
        {
            var data = string.Join(parameterDelimiter.ToString(), new[]
            {
                Format(parameterDelimiter.ToString()), // 1
                Format(recordDelimiter.ToString()),
                Format(ProductIdentification),
                Format(FullFileName),
                Format(ExportingSystem), // 5
                Format(SystemVersion),
                Format(IntegerSize),
                Format(SinglePrecision),
                Format(SignificantDecimalDigits),
                Format(DoublePrecisionMagnitude), // 10
                Format(DoublePrecisionSignificance),
                Format(Identifier),
                Format(ModelSpaceScale),
                Format((int)ModelUnits),
                Format(CustomModelUnits), // 15
                Format(MaxLineWeightGraduations),
                Format(MaxLineWidth),
                Format(TimeStamp),
                Format(Epsilon),
                Format(ApproxLargestCoordinateValue), // 20
                Format(Author),
                Format(Organization),
                Format((int)Version),
                Format((int)DraftingStandard),
                Format(ModifiedDate), // 25
                Format(ApplicationProtocol),
            });
            return SplitString(data);
        }
    }
}
