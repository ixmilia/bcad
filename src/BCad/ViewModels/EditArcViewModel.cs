// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class EditArcViewModel : EditEntityViewModel
    {
        private Arc arc;
        private Point center;
        private double startAngle;
        private double endAngle;
        private double radius;
        private Vector normal;
        private double thickness;

        public EditArcViewModel(IWorkspace workspace, Arc arc)
            : base(workspace)
        {
            this.arc = arc;
            center = arc.Center;
            startAngle = arc.StartAngle;
            endAngle = arc.EndAngle;
            radius = arc.Radius;
            normal = arc.Normal;
            thickness = arc.Thickness;
        }

        public Point Center
        {
            get { return center; }
            set
            {
                if (center == value)
                    return;
                center = value;
                ReplaceArc(arc.Update(center: value));
                OnPropertyChanged();
            }
        }

        public double StartAngle
        {
            get { return startAngle; }
            set
            {
                if (startAngle == value)
                    return;
                startAngle = value;
                ReplaceArc(arc.Update(startAngle: value));
                OnPropertyChanged();
            }
        }

        public double EndAngle
        {
            get { return endAngle; }
            set
            {
                if (endAngle == value)
                    return;
                endAngle = value;
                ReplaceArc(arc.Update(endAngle: value));
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
                ReplaceArc(arc.Update(radius: value));
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
                ReplaceArc(arc.Update(normal: value));
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
                ReplaceArc(arc.Update(thickness: value));
                OnPropertyChanged();
            }
        }

        private void ReplaceArc(Arc newArc)
        {
            ReplaceEntity(arc, newArc);
            arc = newArc;
        }
    }
}
