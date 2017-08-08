// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    public abstract class AbstractDrawPolyLineCommand : ICadCommand
    {
        protected abstract bool ClosePolyline { get; }

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var points = new List<Vertex>();
            var segments = new List<PrimitiveLine>();

            var input = await workspace.InputService.GetPoint(new UserDirective("Start"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            points.Add(new Vertex(first));
            Point last = first;
            while (true)
            {
                // TODO: allow adding arcs, too
                var current = await workspace.InputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    var toDraw = segments.Concat(new[] { new PrimitiveLine(last, p) });
                    if (ClosePolyline)
                    {
                        toDraw = toDraw.Concat(new[] { new PrimitiveLine(p, first) });
                    }

                    return toDraw;
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    segments.Add(new PrimitiveLine(last, current.Value));
                    points.Add(new Vertex(current.Value));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        points.Add(new Vertex(first));
                    }
                    break;
                }
                else
                {
                    break;
                }
            }

            if (ClosePolyline)
            {
                points.Add(points.First());
            }

            var polyline = new Polyline(points);
            workspace.AddToCurrentLayer(polyline);
            return true;
        }
    }
}
