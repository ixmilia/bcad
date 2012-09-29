﻿namespace BCad.Entities
{
    public class ProjectedLine : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Line; }
        }

        public Line OriginalLine { get; private set; }

        public Point P1 { get; private set; }

        public Point P2 { get; private set; }

        public ProjectedLine(Line line, Layer layer, Point p1, Point p2)
            : base(layer)
        {
            OriginalLine = line;
            P1 = p1;
            P2 = p2;
        }
    }
}
