// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IxMilia.BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty ShowNamesProperty =
            DependencyProperty.Register(nameof(ShowNames), typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty AllowNullColorProperty =
            DependencyProperty.Register(nameof(AllowNullColor), typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(true, OnAllowNullColorPropertyChanged));
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(CadColor?), typeof(ColorPicker), new FrameworkPropertyMetadata(CadColor.Black, OnSelectedColorPropertyChanged));
        public static readonly DependencyProperty AvailableColorsProperty =
            DependencyProperty.Register(nameof(AvailableColors), typeof(IEnumerable<CadColor>), typeof(ColorPicker), new FrameworkPropertyMetadata(CadColor.Defaults, OnAvailableColorsPropertyChanged));
        public static readonly DependencyProperty PreviewWidthProperty =
            DependencyProperty.Register(nameof(PreviewWidth), typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(15.0));
        public static readonly DependencyProperty PreviewHeightProperty =
            DependencyProperty.Register(nameof(PreviewHeight), typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(15.0));

        public bool ShowNames
        {
            get { return (bool)GetValue(ShowNamesProperty); }
            set { SetValue(ShowNamesProperty, value); }
        }

        public bool AllowNullColor
        {
            get { return (bool)GetValue(AllowNullColorProperty); }
            set { SetValue(AllowNullColorProperty, value); }
        }

        public CadColor? SelectedColor
        {
            get { return (CadColor?)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public IEnumerable<CadColor> AvailableColors
        {
            get { return (IEnumerable<CadColor>)GetValue(AvailableColorsProperty); }
            set { SetValue(AvailableColorsProperty, value); }
        }

        public double PreviewWidth
        {
            get { return (double)GetValue(PreviewWidthProperty); }
            set { SetValue(PreviewWidthProperty, value); }
        }

        public double PreviewHeight
        {
            get { return (double)GetValue(PreviewHeightProperty); }
            set { SetValue(PreviewHeightProperty, value); }
        }

        private static void OnAllowNullColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as ColorPicker;
            if (control != null)
            {
                control.UpdateAvailableColors();
            }
        }

        private static void OnSelectedColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as ColorPicker;
            if (control != null)
            {
                control.comboBox.SelectedItem = e.NewValue ?? new NullColor();
            }
        }

        private static void OnAvailableColorsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = source as ColorPicker;
            if (control != null)
            {
                control._actualItems = (e.NewValue as IEnumerable<CadColor>)?.Cast<object>();
                control.UpdateAvailableColors();
            }
        }

        private IEnumerable<object> _actualItems = CadColor.Defaults.Cast<object>();

        public ColorPicker()
        {
            InitializeComponent();
            UpdateAvailableColors();
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void UpdateAvailableColors()
        {
            if (AllowNullColor)
            {
                var fullList = new List<object>();
                fullList.Add(new NullColor());
                fullList.AddRange(_actualItems);
                comboBox.ItemsSource = fullList;
            }
            else
            {
                comboBox.ItemsSource = _actualItems;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedItem?.GetType() == typeof(CadColor))
            {
                SelectedColor = (CadColor)comboBox.SelectedItem;
            }
            else if (comboBox.SelectedItem?.GetType() == typeof(NullColor))
            {
                SelectedColor = null;
            }
        }

        private class NullColor
        {
            public bool Equals(NullColor other)
            {
                return other != null;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as NullColor);
            }

            public override int GetHashCode()
            {
                return -1;
            }

            public override string ToString()
            {
                return "(Auto)";
            }
        }
    }
}
