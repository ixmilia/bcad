using System;
using System.Windows;
using System.Windows.Controls;

namespace BCad.UI
{
    public abstract class BCadControl : UserControl
    {
        private Window windowParent;

        public void SetWindowParent(Window window)
        {
            this.windowParent = window;
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
            this.windowParent.Hide();
        }

        protected void Show()
        {
            this.windowParent.Show();
        }
    }
}
