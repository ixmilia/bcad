﻿using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace BCad.UI
{
    public class BCadControl : UserControl
    {
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
    }
}
