namespace IxMilia.BCad
{
    public struct SelectionRectangle
    {
        public Point TopLeftScreen { get; private set; }
        public Point BottomRightScreen { get; private set; }
        public Point TopLeftWorld { get; private set; }
        public Point BottomRightWorld { get; private set; }

        public SelectionRectangle(Point topLeftScreen, Point bottomRightScreen, Point topLeftWorld, Point bottomRightWorld)
        {
            TopLeftScreen = topLeftScreen;
            BottomRightScreen = bottomRightScreen;
            TopLeftWorld = topLeftWorld;
            BottomRightWorld = bottomRightWorld;
        }
    }
}
