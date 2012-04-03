using System;

namespace BCad
{
    public interface IDialogFactory
    {
        bool? ShowDialog(Type type);
    }
}
