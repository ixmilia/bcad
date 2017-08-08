// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace IxMilia.BCad.UI
{
    /// <summary>
    /// Interaction logic for EditablePoint.xaml
    /// </summary>
    public partial class EditablePoint : UserControl
    {
        public static DependencyProperty PointProperty =
            DependencyProperty.Register("Point", typeof(Point), typeof(EditablePoint), new PropertyMetadata(new Point(), OnPointPropertyChanged));
        public static DependencyProperty UnitFormatProperty =
            DependencyProperty.Register("UnitFormat", typeof(UnitFormat), typeof(EditablePoint), new PropertyMetadata(UnitFormat.Metric, OnUnitFormatPropertyChanged));
        public static DependencyProperty UnitPrecisionProperty =
            DependencyProperty.Register("UnitPrecision", typeof(int), typeof(EditablePoint), new PropertyMetadata(16, OnUnitPrecisionPropertyChanged));

        private static void OnPointPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as EditablePoint;
            if (control != null)
            {
                control.RecalcString();
            }
        }

        private static void OnUnitFormatPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as EditablePoint;
            if (control != null)
            {
                control.RecalcString();
            }
        }

        private static void OnUnitPrecisionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as EditablePoint;
            if (control != null)
            {
                control.RecalcString();
            }
        }

        public Point Point
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        public UnitFormat UnitFormat
        {
            get { return (UnitFormat)GetValue(UnitFormatProperty); }
            set { SetValue(UnitFormatProperty, value); }
        }

        public int UnitPrecision
        {
            get { return (int)GetValue(UnitPrecisionProperty); }
            set { SetValue(UnitPrecisionProperty, value); }
        }

        private EditablePointViewModel viewModel;
        private bool dontParse;

        public EditablePoint()
        {
            InitializeComponent();
            viewModel = new EditablePointViewModel();
            viewModel.PropertyChanged += (sender, e) => ReparsePoint();
            grid.DataContext = viewModel;
        }

        private void RecalcString()
        {
            dontParse = true;
            viewModel.PointString = string.Format("{0},{1},{2}",
                DrawingSettings.FormatUnits(Point.X, UnitFormat, UnitPrecision),
                DrawingSettings.FormatUnits(Point.Y, UnitFormat, UnitPrecision),
                DrawingSettings.FormatUnits(Point.Z, UnitFormat, UnitPrecision));
            dontParse = false;
        }

        private void ReparsePoint()
        {
            if (dontParse)
                return;

            // TODO: make this more robust
            var parts = viewModel.PointString.Split(",".ToCharArray());
            if (parts.Length == 3)
            {
                double value;
                var x = 0.0;
                var y = 0.0;
                var z = 0.0;
                if (DrawingSettings.TryParseUnits(parts[0], out value))
                    x = value;
                if (DrawingSettings.TryParseUnits(parts[1], out value))
                    y = value;
                if (DrawingSettings.TryParseUnits(parts[2], out value))
                    z = value;
                Point = new Point(x, y, z);
            }
        }

        private class EditablePointViewModel : INotifyPropertyChanged
        {
            private string pointString;

            public string PointString
            {
                get { return pointString; }
                set
                {
                    if (pointString == value)
                        return;
                    pointString = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                OnPropertyChangedDirect(propertyName);
            }

            private void OnPropertyChangedDirect(string propertyName)
            {
                var changed = PropertyChanged;
                if (changed != null)
                    changed(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
