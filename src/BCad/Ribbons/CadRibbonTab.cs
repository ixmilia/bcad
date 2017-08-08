// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Markup;

namespace IxMilia.BCad.Ribbons
{
    [ContentProperty("Items")]
    public class CadRibbonTab : RibbonTab
    {
        public IWorkspace Workspace { get; private set; }

        protected CadRibbonTab(IWorkspace workspace)
        {
            Workspace = workspace;
            CommandBindings.Add(new CommandBinding(RibbonCommands.RibbonCommands.CADCommand, CommandBinding_Executed, CommandBinding_CanExecute));
        }

        protected void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Workspace == null ? false : Workspace.CanExecute();
        }

        protected void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(Workspace != null, "Workspace should not have been null");
            var command = e.Parameter as string;
            Debug.Assert(command != null, "Command string should not have been null");
            Workspace.ExecuteCommand(command);
        }
    }
}
