using System;

namespace BCad
{
    public interface IDialogFactory
    {
        bool? ShowDialog(string type, string id);
    }
}
