// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class EditLocationViewModel : EditEntityViewModel
    {
        private Location location;
        private Point point;

        public EditLocationViewModel(IWorkspace workspace, Location location)
            : base(workspace)
        {
            this.location = location;
            point = location.Point;
        }

        public Point Point
        {
            get { return point; }
            set
            {
                if (point == value)
                    return;
                point = value;
                ReplacePoint(location.Update(point: value));
                OnPropertyChanged();
            }
        }

        private void ReplacePoint(Location newLocation)
        {
            ReplaceEntity(location, newLocation);
            location = newLocation;
        }
    }
}
