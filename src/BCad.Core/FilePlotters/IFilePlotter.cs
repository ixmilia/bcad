// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using BCad.Entities;

namespace BCad.FilePlotters
{
    public interface IFilePlotter
    {
        void Plot(IEnumerable<ProjectedEntity> entities, double width, double height, Stream stream);
    }
}
