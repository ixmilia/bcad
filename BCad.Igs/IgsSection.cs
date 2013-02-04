using System.Collections.Generic;

namespace BCad.Igs
{
    internal enum IgsSectionType
    {
        Start,
        Global,
        DirectoryEntry,
        ParameterData,
        Terminate
    }

    internal abstract class IgsSection
    {
        protected const int MaxDataLength = 72;

        protected abstract IgsSectionType SectionType { get; }

        protected abstract IEnumerable<string> GetData();
    }
}
