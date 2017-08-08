// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class MutableLayerViewModel : INotifyPropertyChanged
    {
        private bool? isVisible = true;
        private CadColor? color;
        private string name;
        private Layer layer;

        public MutableLayerViewModel(string name)
        {
            this.name = name;
            this.color = null;
        }

        public MutableLayerViewModel(Layer layer)
        {
            this.layer = layer;
            this.name = layer.Name;
            this.color = layer.Color;
            this.isVisible = layer.IsVisible;
        }

        public bool? IsVisible
        {
            get { return isVisible; }
            set
            {
                if (isVisible == value)
                    return;
                isVisible = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("IsDirty");
            }
        }

        public CadColor? Color
        {
            get { return color; }
            set
            {
                if (color == value)
                    return;
                color = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("IsDirty");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("IsDirty");
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.layer == null
                    ? true
                    : this.Name != this.layer.Name ||
                      this.IsVisible != this.layer.IsVisible ||
                      this.Color != this.layer.Color;
            }
        }

        public Layer GetUpdatedLayer()
        {
            if (this.layer == null)
            {
                return new Layer(this.Name, color: Color, isVisible: IsVisible ?? false);
            }
            else if (this.IsDirty)
            {
                return this.layer.Update(name: this.Name, color: this.Color, isVisible: this.IsVisible ?? false);
            }
            else
            {
                return this.layer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedDirect(propertyName);
        }

        protected void OnPropertyChangedDirect(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
