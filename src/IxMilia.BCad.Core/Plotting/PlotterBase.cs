using System.IO;

namespace IxMilia.BCad.Plotting
{
    public abstract class PlotterBase
    {
        public abstract void Plot(Drawing drawing, ViewPort viewPort, Stream outputStream);

        public static double ApplyScaleToThickness(double thicnkess, double scale)
        {
            return double.IsNaN(scale)
                ? 0.0
                : thicnkess * scale;
        }
    }
}
