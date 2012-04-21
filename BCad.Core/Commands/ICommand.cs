using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Commands
{
    public interface ICommand
    {
        bool Execute(object arg = null);
        string DisplayName { get; }
    }
}
