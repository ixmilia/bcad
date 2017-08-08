// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace IxMilia.BCad.UI.View
{
    public partial class ViewPane
    {
        public class BindingClass : INotifyPropertyChanged
        {
            private Brush autoBrush;
            private Brush selectionBrush;
            private Brush snapPointBrush;
            private Brush hotPointBrush;
            private Transform snapPointTransform;
            private double snapPointStrokeThickness;
            private Point cursorScreen;
            private Point cursorWorld;
            private Point leftCursorExtent;
            private Point rightCursorExtent;
            private Point topCursorExtent;
            private Point bottomCursorExtent;
            private Point entitySelectionTopLeft;
            private Point entitySelectionTopRight;
            private Point entitySelectionBottomLeft;
            private Point entitySelectionBottomRight;
            private Point textCursorStart;
            private Point textCursorEnd;
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

            public Brush SnapPointBrush
            {
                get { return snapPointBrush; }
                set
                {
                    if (snapPointBrush == value)
                        return;
                    snapPointBrush = value;
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

            public double SnapPointStrokeThickness
            {
                get { return snapPointStrokeThickness; }
                set
                {
                    if (snapPointStrokeThickness == value)
                        return;
                    snapPointStrokeThickness = value;
                    OnPropertyChanged();
                }
            }

            public Point CursorScreen
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
                    OnPropertyChangedDirect(nameof(CursorWorldString));
                }
            }

            public string CursorWorldString { get { return workspace.Format(CursorWorld); } }

            public Point LeftCursorExtent
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

            public Point RightCursorExtent
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

            public Point TopCursorExtent
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

            public Point BottomCursorExtent
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

            public Point EntitySelectionTopLeft
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

            public Point EntitySelectionTopRight
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

            public Point EntitySelectionBottomLeft
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

            public Point EntitySelectionBottomRight
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

            public Point TextCursorStart
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

            public Point TextCursorEnd
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
