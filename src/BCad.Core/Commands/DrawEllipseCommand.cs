// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Ellipse", "ELLIPSE", "ellipse", "el")]
    internal class DrawEllipseCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawingPlane = workspace.DrawingPlane;
            var center = await workspace.InputService.GetPoint(new UserDirective("Center"));
            if (!center.Cancel && center.HasValue)
            {
                var majorEnd = await workspace.InputService.GetPoint(new UserDirective("Major axis endpoint"), p =>
                {
                    return new[]
                    {
                        new PrimitiveLine(center.Value, p)
                    };
                });
                var majorPrimitive = new PrimitiveLine(center.Value, majorEnd.Value);
                var majorAxis = majorEnd.Value - center.Value;
                var majorAxisLength = majorAxis.Length;
                if (!majorEnd.Cancel && majorEnd.HasValue)
                {
                    var minorEnd = await workspace.InputService.GetPoint(new UserDirective("Minor axis endpoint"), lastPoint: center.Value, onCursorMove: p =>
                    {
                        var tempMinorAxis = p - center.Value;
                        var tempMinorAxisRatio = tempMinorAxis.Length / majorAxisLength;
                        var el = new PrimitiveEllipse(center.Value, majorAxis, drawingPlane.Normal, tempMinorAxisRatio, 0.0, 360.0, null);
                        return new IPrimitive[]
                        {
                            majorPrimitive, // major axis line
                            new PrimitiveLine(center.Value, p), // minor axis line
                            el // the ellipse
                        };
                    });

                    if (!minorEnd.Cancel && minorEnd.HasValue)
                    {
                        var minorAxis = minorEnd.Value - center.Value;
                        var minorAxisRatio = minorAxis.Length / majorAxisLength;
                        if (!minorEnd.Cancel && minorEnd.HasValue)
                        {
                            var el = new Ellipse(center.Value, majorAxis, minorAxisRatio, 0.0, 360.0, drawingPlane.Normal, null);
                            workspace.AddToCurrentLayer(el);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
