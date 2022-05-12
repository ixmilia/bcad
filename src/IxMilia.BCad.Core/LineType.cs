namespace IxMilia.BCad
{
    public class LineType
    {
        public string Name { get; }
        public double[] Pattern { get; }
        public string Description { get; }

        public LineType(string name, double[] pattern, string description)
        {
            Name = name;
            Pattern = pattern;
            Description = description;
        }

        public LineType Update(
            string name = null,
            double[] pattern = null,
            string description = null)
        {
            var newName = name ?? Name;
            var newPattern = pattern ?? Pattern;
            var newDescription = description ?? Description;
            
            if (ReferenceEquals(newName, Name) &&
                ReferenceEquals(newPattern, Pattern) &&
                ReferenceEquals(newDescription, Description))
            {
                return this;
            }

            return new LineType(newName, newPattern, newDescription);
        }
    }
}
