using System.Collections.Generic;

namespace IxMilia.Dxf
{
    public abstract class DxfSymbolTableFlags
    {
        protected int Flags = 0;

        protected abstract string TableType { get; }
        public string Handle { get; set; }

        protected IEnumerable<DxfCodePair> CommonCodePairs()
        {
            yield return new DxfCodePair(0, TableType);
            yield return new DxfCodePair(5, Handle);
            yield return new DxfCodePair(100, "AcDbSymbolTableRecord");
        }

        public DxfSymbolTableFlags()
        {
        }

        public bool ExternallyDependentOnXRef
        {
            get { return DxfHelpers.GetFlag(Flags, 16); }
            set { DxfHelpers.SetFlag(value, ref Flags, 16); }
        }

        public bool ExternallyDependentXRefResolved
        {
            get { return ExternallyDependentOnXRef && DxfHelpers.GetFlag(Flags, 32); }
            set
            {
                ExternallyDependentOnXRef = true;
                DxfHelpers.SetFlag(value, ref Flags, 32);
            }
        }

        public bool ReferencedOnLastEdit
        {
            get { return DxfHelpers.GetFlag(Flags, 64); }
            set { DxfHelpers.SetFlag(value, ref Flags, 64); }
        }
    }
}
