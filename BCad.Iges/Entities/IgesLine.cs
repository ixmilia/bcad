using System;

namespace BCad.Iges.Entities
{
    public class IgesLine : IgesEntity
    {
        public override IgesEntityType Type { get { return IgesEntityType.Line; } }

        public override int LineCount { get { return 1; } }

        public IgesBounding Bounding { get; set; }

        public IgesPoint P1 { get; set; }
        public IgesPoint P2 { get; set; }

        public IgesLine()
        {
            Bounding = IgesBounding.BoundOnBothSides;
        }
    }
}
