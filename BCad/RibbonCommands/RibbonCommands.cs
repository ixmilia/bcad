using System.Windows.Input;

namespace BCad.RibbonCommands
{
    public class RibbonCommands
    {
        private static RoutedUICommand cadCommand = new RoutedUICommand("Execute CAD Command", "ExecuteCADCommand", typeof(RibbonCommands));

        public static RoutedUICommand CADCommand
        {
            get { return cadCommand; }
        }
    }
}
