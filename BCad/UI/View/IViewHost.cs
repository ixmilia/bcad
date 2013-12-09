namespace BCad.UI
{
    public interface IViewHost : IViewControl
    {
        int DisplayHeight { get; }
        int DisplayWidth { get; }
    }
}
