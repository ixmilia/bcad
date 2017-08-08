// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Draw.PolyLine", "POLYLINE", "polyline", "pl")]
    public class DrawPolyLineCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => false;
    }
}
