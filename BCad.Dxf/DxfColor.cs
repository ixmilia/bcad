using System;

namespace BCad.Dxf
{
    public class DxfColor
    {
        public short RawValue { get; set; }

        public bool IsByLayer
        {
            get { return RawValue == 256; }
        }

        public bool IsByBlock { get { return RawValue == 0; } }

        public bool IsTurnedOff { get { return RawValue < 0; } }

        public void SetByLayer()
        {
            RawValue = 256;
        }

        public void SetByBlock()
        {
            RawValue = 0;
        }

        public void TurnOff()
        {
            RawValue = -1;
        }

        public bool IsIndex
        {
            get { return RawValue >= 1 && RawValue <= 255; }
        }

        public byte Index
        {
            get
            {
                if (IsIndex)
                    return (byte)RawValue;
                else
                    throw new NotSupportedException("Color does not have an index.");
            }
        }

        private DxfColor()
            : this(0)
        {
        }

        private DxfColor(byte index)
        {
            this.RawValue = index;
        }

        public override string ToString()
        {
            if (IsByLayer)
                return "BYLAYER";
            else if (IsByBlock)
                return "BYBLOCK";
            else if (IsTurnedOff)
                return "OFF";
            else
                return RawValue.ToString();
        }

        public static DxfColor FromIndex(byte index)
        {
            return new DxfColor(index);
        }

        public static DxfColor FromRawValue(short value)
        {
            return new DxfColor() { RawValue = value };
        }

        public static DxfColor ByLayer
        {
            get
            {
                var c = new DxfColor();
                c.SetByLayer();
                return c;
            }
        }

        public static DxfColor ByBlock
        {
            get
            {
                var c = new DxfColor();
                c.SetByBlock();
                return c;
            }
        }

        public static DxfColor TurnedOff
        {
            get
            {
                var c = new DxfColor();
                c.TurnOff();
                return c;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DxfColor)
            {
                return this.RawValue == ((DxfColor)obj).RawValue;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.RawValue.GetHashCode();
        }
    }
}
