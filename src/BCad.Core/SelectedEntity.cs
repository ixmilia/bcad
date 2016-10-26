// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;

namespace BCad
{
    public class SelectedEntity
    {
        public Entity Entity { get; private set; }

        public Point SelectionPoint { get; private set; }

        public SelectedEntity(Entity entity, Point selectionPoint)
        {
            this.Entity = entity;
            this.SelectionPoint = selectionPoint;
        }
    }
}
