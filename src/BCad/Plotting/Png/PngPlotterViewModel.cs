// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

namespace IxMilia.BCad.Plotting.Png
{
    public class PngPlotterViewModel : ViewPortViewModel
    {
        private Stream _stream;
        public Stream Stream
        {
            get => _stream;
            set => SetValue(ref _stream, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetValue(ref _fileName, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                SetValue(ref _width, value);
                OnPropertyChanged(nameof(ViewWidth));
                OnPropertyChanged(nameof(ViewHeight));
                OnPropertyChanged(nameof(PreviewWidth));
                OnPropertyChanged(nameof(PreviewHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                SetValue(ref _height, value);
                OnPropertyChanged(nameof(ViewWidth));
                OnPropertyChanged(nameof(ViewHeight));
                OnPropertyChanged(nameof(PreviewWidth));
                OnPropertyChanged(nameof(PreviewHeight));
                OnPropertyChanged(nameof(ViewPort));
            }
        }

        public override double ViewWidth => Width;

        public override double ViewHeight => Height;

        public double MaxPreviewSize => 400.0;

        public double PreviewWidth => (ViewWidth / Math.Max(ViewWidth, ViewHeight)) * MaxPreviewSize;

        public double PreviewHeight => (ViewHeight / Math.Max(ViewWidth, ViewHeight)) * MaxPreviewSize;

        public PngPlotterViewModel(IWorkspace workspace)
            : base(workspace)
        {
            Width = 640.0;
            Height = 480.0;
        }
    }
}
