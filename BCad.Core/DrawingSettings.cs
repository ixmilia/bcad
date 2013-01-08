namespace BCad
{
    public class DrawingSettings
    {
        private readonly string fileName;

        public string FileName { get { return this.fileName; } }

        public DrawingSettings()
            : this(null)
        {
        }

        public DrawingSettings(string path)
        {
            this.fileName = path;
        }

        public DrawingSettings Update(string fileName = null)
        {
            return new DrawingSettings(fileName ?? this.fileName);
        }
    }
}
