namespace BCad.Commands
{
    [ExportCadCommand("Draw.Polygon", "POLYGON", "polygon", "pg")]
    public class DrawPolygonCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => true;
    }
}
