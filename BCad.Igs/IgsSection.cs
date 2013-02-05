using System;
using System.Collections.Generic;

namespace BCad.Igs
{
    internal enum IgsSectionType
    {
        None = 0,
        Start = 1,
        Global = 2,
        DirectoryEntry = 3,
        ParameterData = 4,
        Terminate = 5
    }

    internal abstract class IgsSection
    {
        public const int MaxDataLength = 72;

        public abstract IgsSectionType SectionType { get; }

        public abstract IEnumerable<string> GetData();

        protected static IEnumerable<string> SplitString(string data)
        {
            var result = new List<string>();
            if (data != null)
            {
                int index = 0;
                while (index < data.Length)
                {
                    var length = Math.Min(MaxDataLength, data.Length - index);
                    result.Add(data.Substring(index, length));
                    index += length;
                }
            }

            return result;
        }

        protected static string Format(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return string.Format("{0}H{1}", str.Length, str);
        }

        protected static string Format(DateTime date)
        {
            return Format(date.ToString("yyyyMMdd.HHmmss"));
        }

        protected static string Format(double value)
        {
            return value.ToString("E12");
        }

        protected static string Format(int value)
        {
            return value.ToString();
        }
    }
}
