namespace IxMilia.BCad
{
    public struct Optional<T>
    {
        public bool HasValue { get; }
        public T Value { get; }

        public Optional(T value)
        {
            HasValue = true;
            Value = value;
        }

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public T GetValue(T fallback) => HasValue ? Value : fallback;
    }
}
