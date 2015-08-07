using System.ComponentModel;
using BCad.SnapPoints;

namespace BCad
{
    public interface ISettingsManager : INotifyPropertyChanged
    {
        string[] RibbonOrder { get; set; }
        string PlotDialogId { get; set; }
        string LayerDialogId { get; set; }
        string RendererId { get; set; }
        double SnapPointDistance { get; set; }
        double SnapPointSize { get; set; }
        double EntitySelectionRadius { get; set; }
        int CursorSize { get; set; }
        int TextCursorSize { get; set; }
        bool PointSnap { get; set; }
        bool AngleSnap { get; set; }
        bool Ortho { get; set; }
        bool Debug { get; set; }
        double SnapAngleDistance { get; set; }
        double[] SnapAngles { get; set; }
        CadColor BackgroundColor { get; set; }
        CadColor SnapPointColor { get; set; }
        CadColor HotPointColor { get; set; }
        SnapPointKind AllowedSnapPoints { get; set; }
    }
}
