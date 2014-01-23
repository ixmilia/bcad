using System.Threading.Tasks;

namespace BCad
{
    public interface IViewControl
    {
        Point GetCursorPoint();
        int DisplayHeight { get; }
        int DisplayWidth { get; }

        Task<SelectionRectangle> GetSelectionRectangle();
    }
}
