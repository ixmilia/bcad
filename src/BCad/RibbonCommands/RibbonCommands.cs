// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Input;

namespace IxMilia.BCad.RibbonCommands
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
