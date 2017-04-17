// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BCad.FileHandlers
{
    public enum DxfFileVersion
    {
        // not all versions are reflected here
        R12,
        R13,
        R14,
        R2000,
        R2004,
        R2007,
        R2010,
        R2013
    }

    public class DxfFileSettings : INotifyPropertyChanged
    {
        private DxfFileVersion _fileVersion;
        public DxfFileVersion FileVersion
        {
            get => _fileVersion;
            set
            {
                if (_fileVersion == value)
                {
                    return;
                }

                _fileVersion = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
