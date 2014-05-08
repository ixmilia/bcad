namespace BCad
{
    public struct IndexedColor
    {
        public readonly byte Value;

        public bool IsAuto
        {
            get { return this.Value == 0; }
        }

        public IndexedColor(byte value)
        {
            this.Value = value;
        }

        public string DisplayValue
        {
            get { return this.IsAuto ? "Auto" : this.Value.ToString(); }
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsAuto ? "Auto" : Value.ToString();
        }

        public static bool operator ==(IndexedColor a, IndexedColor b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(IndexedColor a, IndexedColor b)
        {
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is IndexedColor)
            {
                return this == (IndexedColor)obj;
            }
            else
            {
                return false;
            }
        }

        public static IndexedColor Default { get { return Auto; } }

        public static IndexedColor Auto { get { return new IndexedColor(0); } }
    }
}
