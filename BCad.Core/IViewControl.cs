using System.Threading.Tasks;

namespace BCad
{
    public interface IViewControl
    {
        Task<Point> GetCursorPoint();
        int DisplayHeight { get; }
        int DisplayWidth { get; }
        Task<SelectionRectangle> GetSelectionRectangle();
    }
}
