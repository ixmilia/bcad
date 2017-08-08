// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IxMilia.BCad.Services;
using IxMilia.BCad.UI;

namespace IxMilia.BCad
{
    [ExportWorkspaceService, Shared]
    internal class DialogFactory : IDialogFactoryService
    {
        [ImportMany]
        public IEnumerable<Lazy<BCadControl, ControlMetadata>> Controls { get; set; }

        public async Task<bool?> ShowDialog(string type, string id, INotifyPropertyChanged viewModel = null)
        {
            var lazyControl = Controls.FirstOrDefault(c => c.Metadata.ControlType == type && c.Metadata.ControlId == id);
            if (lazyControl == null)
            {
                // TODO: throw
                MessageBox.Show("Unable to find specified dialog.");
                return false;
            }

            var control = lazyControl.Value;
            using (var window = new BCadDialog(control))
            {
                window.Title = lazyControl.Metadata.Title;
                window.Owner = Application.Current.MainWindow;
                if (viewModel != null)
                {
                    window.DataContext = viewModel;
                }

                return await window.ShowHideableDialog();
            }
        }
    }
}
