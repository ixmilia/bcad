using BCad.Igs.Entities;

namespace BCad.Igs.Directory
{
    internal class IgsDirectoryData
    {
        public IgsEntityType EntityType { get; set; }
        public int ParameterPointer { get; set; }
        public int Structure { get; set; }
        public int LineFontPattern { get; set; }
        public int Level { get; set; }
        public int View { get; set; }
        public int TransformationMatrixPointer { get; set; }
        public int LableDisplay { get; set; }
        public int StatusNumber { get; set; }

        public int LineWeight { get; set; }
        public IgsColorNumber Color { get; set; }
        public int LineCount { get; set; }
        public int FormNumber { get; set; }
        public string EntityLabel { get; set; }
    }
}
