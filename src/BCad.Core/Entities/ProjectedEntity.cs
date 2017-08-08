// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Entities
{
    public abstract class ProjectedEntity
    {
        public Layer OriginalLayer { get; private set; }

        public abstract EntityKind Kind { get; }

        public ProjectedEntity(Layer layer)
        {
            OriginalLayer = layer;
        }
    }
}
