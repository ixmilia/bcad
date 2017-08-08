// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace IxMilia.BCad
{
    public interface IViewControl
    {
        Task<Point> GetCursorPoint(CancellationToken cancellationToken);
        int DisplayHeight { get; }
        int DisplayWidth { get; }
        Task<SelectionRectangle> GetSelectionRectangle();
    }
}
