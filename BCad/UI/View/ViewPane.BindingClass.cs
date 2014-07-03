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
            private System.Windows.Point cursorScreen;
            private Point cursorWorld;
            private System.Windows.Point leftCursorExtent;
            private System.Windows.Point rightCursorExtent;
            private System.Windows.Point topCursorExtent;
            private System.Windows.Point bottomCursorExtent;
            private System.Windows.Point entitySelectionTopLeft;
            private System.Windows.Point entitySelectionTopRight;
            private System.Windows.Point entitySelectionBottomLeft;
            private System.Windows.Point entitySelectionBottomRight;
            private System.Windows.Point textCursorStart;
            private System.Windows.Point textCursorEnd;
            private IWorkspace workspace;

            public BindingClass(IWorkspace workspace)
            {
                this.workspace = workspace;
            }

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

            public System.Windows.Point CursorScreen
            {
                get { return cursorScreen; }
                set
                {
                    if (cursorScreen == value)
                        return;
                    cursorScreen = value;
                    OnPropertyChanged();
                }
            }

            public Point CursorWorld
            {
                get { return cursorWorld; }
                set
                {
                    if (cursorWorld == value)
                        return;
                    cursorWorld = value;
                    OnPropertyChanged();
                    OnPropertyChangedDirect("CursorWorldString");
                }
            }

            public string CursorWorldString { get { return workspace.Format(CursorWorld); } }

            public System.Windows.Point LeftCursorExtent
            {
                get { return leftCursorExtent; }
                set
                {
                    if (leftCursorExtent == value)
                        return;
                    leftCursorExtent = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point RightCursorExtent
            {
                get { return rightCursorExtent; }
                set
                {
                    if (rightCursorExtent == value)
                        return;
                    rightCursorExtent = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point TopCursorExtent
            {
                get { return topCursorExtent; }
                set
                {
                    if (topCursorExtent == value)
                        return;
                    topCursorExtent = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point BottomCursorExtent
            {
                get { return bottomCursorExtent; }
                set
                {
                    if (bottomCursorExtent == value)
                        return;
                    bottomCursorExtent = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point EntitySelectionTopLeft
            {
                get { return entitySelectionTopLeft; }
                set
                {
                    if (entitySelectionTopLeft == value)
                        return;
                    entitySelectionTopLeft = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point EntitySelectionTopRight
            {
                get { return entitySelectionTopRight; }
                set
                {
                    if (entitySelectionTopRight == value)
                        return;
                    entitySelectionTopRight = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point EntitySelectionBottomLeft
            {
                get { return entitySelectionBottomLeft; }
                set
                {
                    if (entitySelectionBottomLeft == value)
                        return;
                    entitySelectionBottomLeft = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point EntitySelectionBottomRight
            {
                get { return entitySelectionBottomRight; }
                set
                {
                    if (entitySelectionBottomRight == value)
                        return;
                    entitySelectionBottomRight = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point TextCursorStart
            {
                get { return textCursorStart; }
                set
                {
                    if (textCursorStart == value)
                        return;
                    textCursorStart = value;
                    OnPropertyChanged();
                }
            }

            public System.Windows.Point TextCursorEnd
            {
                get { return textCursorEnd; }
                set
                {
                    if (textCursorEnd == value)
                        return;
                    textCursorEnd = value;
                    OnPropertyChanged();
                }
            }

            public void Refresh()
            {
                OnPropertyChangedDirect(string.Empty);
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
