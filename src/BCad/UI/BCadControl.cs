// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;

namespace IxMilia.BCad.UI
{
    public abstract class BCadControl : UserControl
    {
        protected Window WindowParent { get; private set; }

        public void SetWindowParent(Window window)
        {
            WindowParent = window;
        }

        public abstract void OnShowing();

        public virtual void Commit()
        {
        }

        public virtual void Cancel()
        {
        }

        public virtual bool Validate()
        {
            return true;
        }

        protected void Hide()
        {
            this.WindowParent.Hide();
        }

        protected void Show()
        {
            this.WindowParent.Show();
        }
    }
}
