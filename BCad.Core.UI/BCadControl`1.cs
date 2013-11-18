using System.Windows.Controls;

namespace BCad.UI
{
    public abstract class BCadControl<TResult> : UserControl where TResult : class
    {
        public TResult Result { get; protected set; }
    }
}
