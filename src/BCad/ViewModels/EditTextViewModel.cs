// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.Entities;

namespace IxMilia.BCad.ViewModels
{
    public class EditTextViewModel : EditEntityViewModel
    {
        private Text text;
        private string value;
        private Point location;
        private Vector normal;
        private double height;
        private double rotation;

        public EditTextViewModel(IWorkspace workspace, Text text)
            : base(workspace)
        {
            this.text = text;
            value = text.Value;
            location = text.Location;
            normal = text.Normal;
            height = text.Height;
            rotation = text.Rotation;
        }

        public string Value
        {
            get { return value; }
            set
            {
                if (this.value == value)
                    return;
                this.value = value;
                ReplaceText(text.Update(value: value));
                OnPropertyChanged();
            }
        }

        public Point Location
        {
            get { return location; }
            set
            {
                if (location == value)
                    return;
                location = value;
                ReplaceText(text.Update(location: value));
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
                ReplaceText(text.Update(normal: value));
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                if (height == value)
                    return;
                height = value;
                ReplaceText(text.Update(height: value));
                OnPropertyChanged();
            }
        }

        public double Rotation
        {
            get { return rotation; }
            set
            {
                if (rotation == value)
                    return;
                rotation = value;
                ReplaceText(text.Update(rotation: value));
                OnPropertyChanged();
            }
        }

        private void ReplaceText(Text newText)
        {
            ReplaceEntity(text, newText);
            text = newText;
        }
    }
}
