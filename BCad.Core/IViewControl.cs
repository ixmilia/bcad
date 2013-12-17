namespace BCad
{
    public interface IViewControl
    {
        Point GetCursorPoint();
        int DisplayHeight { get; }
        int DisplayWidth { get; }
    }
}
