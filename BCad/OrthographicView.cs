using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using BCad.EventArguments;

namespace BCad
{
    [Export(typeof(IView))]
    internal class OrthographicView : IView
    {
        public OrthographicView()
        {
            BottomLeft = Point.Origin;
            ViewPoint = Point.Origin;
            Sight = Vector.ZAxis;
            Up = Vector.YAxis;
            cursorMove = new MouseEventHandler(OnControlMouseMove);
            sizeChanged = new SizeChangedEventHandler(OnControlSizeChanged);
        }

        public void UpdateView(Point viewPoint = null, Vector sight = null, Vector up = null,
            double? viewWidth = null, Point bottomLeft = null)
        {
            if (viewPoint != null)
                ViewPoint = viewPoint;
            if (sight != null)
                Sight = sight;
            if (up != null)
                Up = up;
            if (viewWidth.HasValue)
                ViewWidth = viewWidth.Value;
            if (bottomLeft != null)
                BottomLeft = bottomLeft;
            OnViewPortChanged(new ViewPortChangedEventArgs(this));
        }

        public event ViewPortChangedEvent ViewPortChanged;

        protected virtual void OnViewPortChanged(ViewPortChangedEventArgs e)
        {
            var cam = new OrthographicCamera(ViewPoint.ToPoint3D(),
                Sight.ToVector3D(),
                Up.ToVector3D(),
                ViewWidth);
            transform = cam.Transform.Value;
            // find scale
            var scale = registeredControl.ActualWidth != 0.0 ? registeredControl.ActualWidth / ViewWidth : 1.0;
            transform.Translate(new Vector3D(-BottomLeft.X, -BottomLeft.Y, 0));
            transform.Scale(new Vector3D(scale, scale, scale));
            transform.Translate(new Vector3D(0, -registeredControl.ActualHeight, 0));
            transform.Scale(new Vector3D(1, -1, 1));
            inverse = transform;
            if (inverse.HasInverse)
                inverse.Invert();

            if (ViewPortChanged != null)
                ViewPortChanged(this, e);
        }

        public Point ViewPoint { get; private set; }

        public Vector Up { get; private set; }

        public Vector Sight { get; private set; }

        public double ViewWidth { get; private set; }

        public Point BottomLeft { get; private set; }

        private object pointGate = new object();

        private System.Windows.Point cursorPoint = new System.Windows.Point();

        public Point GetCursorPoint()
        {
            var p = Mouse.GetPosition(registeredControl);
            return ControlToWorld(new Point(p));
            //lock (pointGate)
            //{
            //    return ControlToWorld(new Point(cursorPoint));
            //}
        }

        private Control registeredControl = null;

        public Control RegisteredControl
        {
            get { return registeredControl; }
            set
            {
                if (registeredControl != null)
                {
                    //registeredControl.MouseMove -= cursorMove;
                    registeredControl.SizeChanged -= sizeChanged;
                }
                registeredControl = value;
                //registeredControl.MouseMove += cursorMove;
                registeredControl.SizeChanged += sizeChanged;
                OnViewPortChanged(new ViewPortChangedEventArgs(this));
            }
        }

        private MouseEventHandler cursorMove;

        private SizeChangedEventHandler sizeChanged;

        private void OnControlMouseMove(object sender, MouseEventArgs e)
        {
            var controlPoint = e.GetPosition(registeredControl);
            lock (pointGate)
            {
                cursorPoint = controlPoint;
            }
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnViewPortChanged(new ViewPortChangedEventArgs(this));
        }

        private Matrix3D transform = Matrix3D.Identity;
        private Matrix3D inverse = Matrix3D.Identity;

        public Point ControlToWorld(Point point)
        {
            return new Point(inverse.Transform(point.ToPoint3D()));
        }

        public Point WorldToControl(Point point)
        {
            return new Point(transform.Transform(point.ToPoint3D()));
        }
    }
}
