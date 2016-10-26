// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Polygon", "POLYGON", "polygon", "pg")]
    public class DrawPolygonCommand : AbstractDrawPolyLineCommand
    {
        protected override bool ClosePolyline => true;
    }
}
