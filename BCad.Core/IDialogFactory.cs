using System;
using System.Threading.Tasks;

namespace BCad
{
    public interface IDialogFactory
    {
        Task<bool?> ShowDialog(string type, string id);
    }
}
