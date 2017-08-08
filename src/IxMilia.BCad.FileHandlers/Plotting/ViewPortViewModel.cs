// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.BCad.Plotting
{
    public abstract class ViewPortViewModel : ViewModelBase
    {
        public IWorkspace Workspace { get; }

        public abstract double ViewHeight { get; }
        public abstract double ViewWidth { get; }

        private PlotScalingType _scalingType;
        public PlotScalingType ScalingType
        {
            get => _scalingType;
            set
            {
                SetValue(ref _scalingType, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private PlotViewPortType _viewPortType;
        public PlotViewPortType ViewPortType
        {
            get => _viewPortType;
            set
            {
                SetValue(ref _viewPortType, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _scaleA;
        public double ScaleA
        {
            get => ScalingType == PlotScalingType.Absolute ? _scaleA : 1.0;
            set
            {
                SetValue(ref _scaleA, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _scaleB;
        public double ScaleB
        {
            get => ScalingType == PlotScalingType.Absolute ? _scaleB : 1.0;
            set
            {
                SetValue(ref _scaleB, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private Point _bottomLeft;
        public Point BottomLeft
        {
            get => _bottomLeft;
            set
            {
                SetValue(ref _bottomLeft, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private Point _topRight;
        public Point TopRight
        {
            get => _topRight;
            set
            {
                SetValue(ref _topRight, value);
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        public ViewPort ViewPort
        {
            get
            {
                ViewPort vp;
                switch (ViewPortType)
                {
                    case PlotViewPortType.Extents:
                        vp = Workspace.Drawing.ShowAllViewPort(
                            Workspace.ActiveViewPort.Sight,
                            Workspace.ActiveViewPort.Up,
                            ViewWidth,
                            ViewHeight,
                            pixelBuffer: 0);
                        break;
                    case PlotViewPortType.Window:
                        vp = new ViewPort(BottomLeft, Workspace.ActiveViewPort.Sight, Workspace.ActiveViewPort.Up, TopRight.Y - BottomLeft.Y);
                        break;
                    default:
                        throw new InvalidOperationException("unsupported viewport type");
                }

                switch (ScalingType)
                {
                    case PlotScalingType.Absolute:
                        vp = vp.Update(viewHeight: ViewHeight * ScaleB / ScaleA);
                        break;
                    case PlotScalingType.ToFit:
                        break;
                    default:
                        throw new InvalidOperationException("unsupported scaling type");
                }

                return vp;
            }
        }

        public ViewPortViewModel(IWorkspace workspace)
        {
            Workspace = workspace;
            ScalingType = PlotScalingType.ToFit;
            ViewPortType = PlotViewPortType.Extents;
            ScaleA = 1.0;
            ScaleB = 1.0;
            BottomLeft = new Point(0.0, 0.0, 0.0);
            TopRight = new Point(1.0, 1.0, 0.0);
        }

        public void UpdateViewWindow(Point bottomLeft, Point topRight)
        {
            _bottomLeft = bottomLeft;
            _topRight = topRight;
            OnPropertyChanged(nameof(BottomLeft));
            OnPropertyChanged(nameof(TopRight));
            OnPropertyChanged(nameof(ViewPort));
        }
    }
}
