namespace BCad.Commands
{
    [ExportCadCommand("Draw.PolyLine", "POLYLINE", "polyline", "pl")]
    public class DrawPolyLineCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => false;
    }
}
