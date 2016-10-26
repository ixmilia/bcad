// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

        public static ValueOrDirective<T> GetCancel()
        {
            return new ValueOrDirective<T>() { Cancel = true };
        }

        public static ValueOrDirective<T> GetDirective(string directive)
        {
            var result = new ValueOrDirective<T>();
            result.Directive = directive;
            result.Cancel = false;
            result.HasValue = false;
            return result;
        }

        public static ValueOrDirective<T> GetValue(T value)
        {
            var result = new ValueOrDirective<T>();
            result.value = value;
            result.Cancel = false;
            result.HasValue = true;
            return result;
        }
    }
}
