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
            return IgsSection.SplitString(this.Data);
        }
    }
}
