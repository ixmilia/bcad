using System.ComponentModel;

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
        RealColor BackgroundColor { get; set; }
        RealColor SnapPointColor { get; set; }
        RealColor HotPointColor { get; set; }
        ColorMap ColorMap { get; set; }
    }
}
