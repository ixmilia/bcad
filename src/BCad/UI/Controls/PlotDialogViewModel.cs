// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using IxMilia.BCad.Plotting;

namespace IxMilia.BCad.UI.Controls
{
    internal class PlotDialogViewModel : INotifyPropertyChanged
    {
        public IEnumerable<PlotterFactoryMetadata> AvailableFactories { get; }

        private PlotterFactoryMetadata _selectedFactory;
        public PlotterFactoryMetadata SelectedFactory
        {
            get => _selectedFactory;
            set
            {
                _selectedFactory = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PlotDialogViewModel(IEnumerable<PlotterFactoryMetadata> availableFactories)
        {
            AvailableFactories = availableFactories.OrderBy(f => f.DisplayName);
            SelectedFactory = AvailableFactories.First();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
