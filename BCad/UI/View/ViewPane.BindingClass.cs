using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace BCad.UI
{
    public partial class ViewPane
    {
        public class BindingClass : INotifyPropertyChanged
        {
            private Brush autoBrush;
            private Brush selectionBrush;
            private Pen cursorPen;
            private Pen snapPointPen;
            private Brush hotPointBrush;
            private Transform snapPointTransform;

            public Brush AutoBrush
            {
                get { return autoBrush; }
                set
                {
                    if (autoBrush == value)
                        return;
                    autoBrush = value;
                    OnPropertyChanged();
                }
            }

            public Brush SelectionBrush
            {
                get { return selectionBrush; }
                set
                {
                    if (selectionBrush == value)
                        return;
                    selectionBrush = value;
                    OnPropertyChanged();
                }
            }

            public Pen CursorPen
            {
                get { return cursorPen; }
                set
                {
                    if (cursorPen == value)
                        return;
                    cursorPen = value;
                    OnPropertyChanged();
                }
            }

            public Pen SnapPointPen
            {
                get { return snapPointPen; }
                set
                {
                    if (snapPointPen == value)
                        return;
                    snapPointPen = value;
                    OnPropertyChanged();
                }
            }

            public Brush HotPointBrush
            {
                get { return hotPointBrush; }
                set
                {
                    if (hotPointBrush == value)
                        return;
                    hotPointBrush = value;
                    OnPropertyChanged();
                }
            }

            public Transform SnapPointTransform
            {
                get { return snapPointTransform; }
                set
                {
                    if (snapPointTransform == value)
                        return;
                    snapPointTransform = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                OnPropertyChangedDirect(propertyName);
            }

            private void OnPropertyChangedDirect(string propertyName)
            {
                var changed = PropertyChanged;
                if (changed != null)
                    changed(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
