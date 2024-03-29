namespace IxMilia.BCad.Rpc
{
    public enum DisplayUpdate
    {
        ZoomIn,
        ZoomOut,
        PanLeft,
        PanRight,
        PanUp,
        PanDown,
    }

    public struct CursorLocation
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class ServerUpdate
    {
        public DisplayUpdate? DisplayUpdate { get; set; }
        public CursorLocation? CursorLocation { get; set; }
    }
}
