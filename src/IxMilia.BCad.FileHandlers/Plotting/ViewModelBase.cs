using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IxMilia.BCad.Plotting
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected void SetValue<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, newValue))
            {
                return;
            }

            backingField = newValue;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
