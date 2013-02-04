using System;
using System.Collections.Generic;

namespace BCad.Igs
{
    internal class IgsStartSection : IgsSection
    {
        public string Data { get; set; }

        public IgsStartSection()
        {
        }

        protected override IgsSectionType SectionType
        {
            get { return IgsSectionType.Start; }
        }

        public override IEnumerable<string> GetData()
        {
            var data = new List<string>();
            if (Data != null)
            {
                int index = 0;
                while (index < Data.Length)
                {
                    var length = Math.Min(MaxDataLength, Data.Length - index);
                    data.Add(Data.Substring(index, length));
                    index += length;
                }
            }

            return data;
        }
    }
}
