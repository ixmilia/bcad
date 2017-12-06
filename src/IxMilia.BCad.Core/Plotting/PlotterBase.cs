// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
