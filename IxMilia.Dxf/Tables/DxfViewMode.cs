namespace IxMilia.Dxf
{
    public struct DxfViewMode
    {
        private int Flags;

        internal int Value { get { return Flags; } }

        internal DxfViewMode(int falgs)
            : this()
        {
            Flags = falgs;
        }

        public bool PerspectiveViewActive
        {
            get { return DxfHelpers.GetFlag(Flags, 1); }
            set { DxfHelpers.SetFlag(value, ref Flags, 1); }
        }

        public bool FrontClippingOn
        {
            get { return DxfHelpers.GetFlag(Flags, 2); }
            set { DxfHelpers.SetFlag(value, ref Flags, 2); }
        }

        public bool BackClippingOn
        {
            get { return DxfHelpers.GetFlag(Flags, 4); }
            set { DxfHelpers.SetFlag(value, ref Flags, 4); }
        }

        public bool UcsFollowModeOn
        {
            get { return DxfHelpers.GetFlag(Flags, 8); }
            set { DxfHelpers.SetFlag(value, ref Flags, 8); }
        }

        public bool FrontClippingAtEye
        {
            get { return !DxfHelpers.GetFlag(Flags, 16); }
            set { DxfHelpers.SetFlag(!value, ref Flags, 16); }
        }
    }
}
