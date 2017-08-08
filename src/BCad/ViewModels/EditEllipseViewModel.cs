// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class EditEllipseViewModel : EditEntityViewModel
    {
        private Ellipse ellipse;
        private Point center;
        private double startAngle;
        private double endAngle;
        private Vector majorAxis;
        private double minorAxisRatio;
        private Vector normal;
        private double thickness;

        public EditEllipseViewModel(IWorkspace workspace, Ellipse ellipse)
            : base(workspace)
        {
            this.ellipse = ellipse;
            center = ellipse.Center;
            startAngle = ellipse.StartAngle;
            endAngle = ellipse.EndAngle;
            majorAxis = ellipse.MajorAxis;
            minorAxisRatio = ellipse.MinorAxisRatio;
            normal = ellipse.Normal;
            thickness = ellipse.Thickness;
        }

        public Point Center
        {
            get { return center; }
            set
            {
                if (center == value)
                    return;
                center = value;
                ReplaceEllipse(ellipse.Update(center: value));
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
                ReplaceEllipse(ellipse.Update(startAngle: value));
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
                ReplaceEllipse(ellipse.Update(endAngle: value));
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
                ReplaceEllipse(ellipse.Update(normal: value));
                OnPropertyChanged();
            }
        }

        public Vector MajorAxis
        {
            get { return majorAxis; }
            set
            {
                if (majorAxis == value)
                    return;
                majorAxis = value;
                ReplaceEllipse(ellipse.Update(majorAxis: value));
                OnPropertyChanged();
                OnPropertyChangedDirect(nameof(Eccentricity));
            }
        }

        public double MinorAxisRatio
        {
            get { return minorAxisRatio; }
            set
            {
                if (minorAxisRatio == value)
                    return;
                minorAxisRatio = value;
                ReplaceEllipse(ellipse.Update(minorAxisRatio: value));
                OnPropertyChanged();
                OnPropertyChangedDirect(nameof(Eccentricity));
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
                ReplaceEllipse(ellipse.Update(thickness: value));
                OnPropertyChanged();
            }
        }

        public double Eccentricity
        {
            get
            {
                return Math.Sqrt(1.0 - (minorAxisRatio * minorAxisRatio));
            }
        }

        private void ReplaceEllipse(Ellipse newEllipse)
        {
            ReplaceEntity(ellipse, newEllipse);
            ellipse = newEllipse;
        }
    }
}
