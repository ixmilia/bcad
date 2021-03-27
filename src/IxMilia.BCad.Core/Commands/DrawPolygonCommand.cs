namespace IxMilia.BCad.Commands
{
    public class DrawPolygonCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => true;
    }
}
