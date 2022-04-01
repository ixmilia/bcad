using System;
using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.Plotting
{
    public abstract class PlotterBase
    {
        public abstract Task Plot(Drawing drawing, ViewPort viewPort, Stream outputStream, Func<string, Task<byte[]>> contentResolver);

        public static double ApplyScaleToThickness(double thicnkess, double scale)
        {
            return double.IsNaN(scale)
                ? 0.0
                : thicnkess * scale;
        }
    }
}
