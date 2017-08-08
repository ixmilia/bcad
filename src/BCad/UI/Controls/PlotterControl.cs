// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace IxMilia.BCad.UI.Controls
{
    public abstract class PlotterControl : UserControl
    {
        private Window _windowParent;

        public void SetWindowParent(Window window)
        {
            _windowParent = window;
        }

        protected void Hide()
        {
            _windowParent.Hide();
        }

        protected void Show()
        {
            _windowParent.Show();
        }

        public abstract void OnShowing();
        public abstract void BeforeCommit();
        public abstract void AfterCommit();
    }
}
