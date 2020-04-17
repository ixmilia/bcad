namespace IxMilia.BCad.Plotting
{
    public abstract class PlotterBase
    {
        public abstract void Plot(IWorkspace workspace);

        public static double ApplyScaleToThickness(double thicnkess, double scale)
        {
            return double.IsNaN(scale)
                ? 0.0
                : thicnkess * scale;
        }
    }
}
