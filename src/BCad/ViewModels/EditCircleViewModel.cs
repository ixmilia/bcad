// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class EditCircleViewModel : EditEntityViewModel
    {
        private Circle circle;
        private Point center;
        private double radius;
        private Vector normal;
        private double thickness;

        public EditCircleViewModel(IWorkspace workspace, Circle circle)
            : base(workspace)
        {
            this.circle = circle;
            center = circle.Center;
            radius = circle.Radius;
            normal = circle.Normal;
            thickness = circle.Thickness;
        }

        public Point Center
        {
            get { return center; }
            set
            {
                if (center == value)
                    return;
                center = value;
                ReplaceCircle(circle.Update(center: value));
                OnPropertyChanged();
            }
        }

        public double Radius
        {
            get { return radius; }
            set
            {
                if (radius == value)
                    return;
                radius = value;
                ReplaceCircle(circle.Update(radius: value));
                OnPropertyChanged();
            }
        }

        public Vector Normal
        {
            get { return normal; }
            set
            {
                if (normal == value)
                    return;
                normal = value;
                ReplaceCircle(circle.Update(normal: value));
                OnPropertyChanged();
            }
        }

        public double Thickness
        {
            get { return thickness; }
            set
            {
                if (thickness == value)
                    return;
                thickness = value;
                ReplaceCircle(circle.Update(thickness: value));
                OnPropertyChanged();
            }
        }

        private void ReplaceCircle(Circle newCircle)
        {
            ReplaceEntity(circle, newCircle);
            circle = newCircle;
        }
    }
}
