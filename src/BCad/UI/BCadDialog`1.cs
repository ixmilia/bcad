using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BCad.UI
{
    public class BCadDialog<TResult> : Window where TResult : class
    {
        public BCadControl<TResult> Control { get; private set; }

        public BCadDialog(BCadControl<TResult> control)
        {
            this.Control = control;
            this.Content = control;
        }

        public BCadDialogResult<TResult> GetDialogResult()
        {
            if (this.ShowDialog() == true)
                return new BCadDialogResult<TResult>(this.Control.Result);
            else
                return new BCadDialogResult<TResult>();
        }
    }
}
