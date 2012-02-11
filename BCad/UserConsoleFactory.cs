using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace BCad
{
    [Export(typeof(IUserConsoleFactory))]
    internal class UserConsoleFactory : IUserConsoleFactory
    {
        public UserControl Generate()
        {
            var console = new WpfConsole();
            App.Container.SatisfyImportsOnce(console);
            return console;
        }
    }
}
