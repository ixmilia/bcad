// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IxMilia.BCad.UI
{
    /// <summary>
    /// Interaction logic for BCadDialog.xaml
    /// </summary>
    public partial class BCadDialog : Window, IDisposable
    {
        public BCadControl Control { get; private set; }

        private TaskCompletionSource<bool?> completionAwaiter = new TaskCompletionSource<bool?>();
        private bool acceptedResult = false;

        public BCadDialog(BCadControl control)
        {
            InitializeComponent();

            this.Control = control;
            this.Control.SetWindowParent(this);
            this.controlSurface.Content = this.Control;
        }

        public void Dispose()
        {
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.Control.Validate())
            {
                acceptedResult = true;
                this.Control.Commit();
                this.Control = null;
                this.Close();
                completionAwaiter.SetResult(true);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            acceptedResult = true;
            this.Control.Cancel();
            completionAwaiter.SetResult(false);
            Cancel();
        }

        private void Cancel()
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key == (int)Key.Escape)
            {
                Cancel();
            }
        }

        public Task<bool?> ShowHideableDialog()
        {
            Control.OnShowing();
            this.Show();
            return completionAwaiter.Task;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!acceptedResult)
            {
                this.Control.Cancel();
                completionAwaiter.SetResult(false);
            }
        }
    }
}
