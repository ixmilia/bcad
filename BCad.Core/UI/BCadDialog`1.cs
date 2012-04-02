using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BCad.UI
{
    public class BCadDialog<TResult> : Window
    {
        public BCadDialog()
        {
        }

        public TResult ShowDialogResult()
        {
            return default(TResult);
        }
    }
}
