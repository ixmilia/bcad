namespace IxMilia.Dxf
{
    public abstract class DxfSymbolTableFlags
    {
        protected int Flags = 0;

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
