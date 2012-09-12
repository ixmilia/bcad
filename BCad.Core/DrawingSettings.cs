namespace BCad
{
    public class DrawingSettings
    {
        private readonly string fileName;
        private readonly bool isDirty;

        public string FileName { get { return this.fileName; } }

        public bool IsDirty { get { return this.isDirty; } }

        public DrawingSettings()
            : this(null)
        {
        }

        public DrawingSettings(string path)
            : this(path, false)
        {
        }

        public DrawingSettings(string path, bool isDirty)
        {
            this.fileName = path;
            this.isDirty = isDirty;
        }

        public DrawingSettings Update(string fileName = null, bool? isDirty = null)
        {
            return new DrawingSettings(
                fileName ?? this.fileName,
                isDirty ?? this.isDirty);
        }
    }
}
