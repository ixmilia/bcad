using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Commands
{
    public interface ICommand
    {
        Task<bool> Execute(object arg = null);
        string DisplayName { get; }
    }
}
