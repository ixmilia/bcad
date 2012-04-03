using System.Windows.Controls;

namespace BCad.UI
{
    public abstract class BCadControl : UserControl
    {
        public abstract void Initialize();

        public abstract void Commit();

        public abstract void Cancel();
    }
}
