using System.Threading;
using System.Threading.Tasks;

namespace IxMilia.BCad
{
    public interface IViewControl
    {
        Task<Point> GetCursorPoint(CancellationToken cancellationToken);
        int DisplayHeight { get; }
        int DisplayWidth { get; }
        Task<SelectionRectangle?> GetSelectionRectangle();
    }
}
