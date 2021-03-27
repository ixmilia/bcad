namespace IxMilia.BCad.Commands
{
    public class DrawPolyLineCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => false;
    }
}
