// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.UI.View
{
    public partial class RenderCanvas
    {
        private class BindingClass : INotifyPropertyChanged
        {
            private double thickness = 0.0;
            public double Thickness
            {
                get { return thickness; }
                set
                {
                    if (thickness == value)
                        return;
                    thickness = value;
                    OnPropertyChanged();
                }
            }

            private ScaleTransform scale = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };
            public ScaleTransform Scale
            {
                get { return scale; }
                set
                {
                    if (scale == value)
                        return;
                    scale = value;
                    OnPropertyChanged();
                }
            }

            private Brush autoBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            public Brush AutoBrush
            {
                get { return autoBrush; }
                set
                {
                    if (autoBrush == value)
                        return;
                    autoBrush = value;
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
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
