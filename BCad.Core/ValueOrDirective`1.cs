using System;

namespace BCad
{
    public struct ValueOrDirective<T>
    {
        private T value;
        public string Directive { get; private set; }
        public bool HasValue { get; private set; }
        public bool Cancel { get; private set; }

        public T Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException("There is no value to get");
                return this.value;
            }
        }

        public ValueOrDirective(string directive)
            : this()
        {
            Cancel = false;
            Directive = directive;
            HasValue = false;
        }

        public ValueOrDirective(T value)
            : this()
        {
            Cancel = false;
            this.value = value;
            HasValue = true;
        }

        public static ValueOrDirective<T> GetCancel()
        {
            return new ValueOrDirective<T>() { Cancel = true };
        }
    }
}
