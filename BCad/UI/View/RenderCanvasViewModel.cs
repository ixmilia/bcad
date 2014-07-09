using System.ComponentModel;
using System.Runtime.CompilerServices;
using BCad.Collections;
using BCad.Entities;

namespace BCad.UI.View
{
    public class RenderCanvasViewModel : INotifyPropertyChanged
    {
        private RealColor backgroundColor = RealColor.Black;
        private ViewPort viewPort = ViewPort.CreateDefaultViewPort();
        private Drawing drawing = new Drawing();
        private double pointSize = 15.0;
        private ObservableHashSet<Entity> selectedEntities = new ObservableHashSet<Entity>();
        private ColorMap colorMap = ColorMap.Default;
        private Point cursorPoint = new Point();
        private RubberBandGenerator rubberBandGenerator = null;

        public RealColor BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (backgroundColor == value)
                    return;
                backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public ViewPort ViewPort
        {
            get { return viewPort; }
            set
            {
                if (viewPort == value)
                    return;
                viewPort = value;
                OnPropertyChanged();
            }
        }

        public Drawing Drawing
        {
            get { return drawing; }
            set
            {
                if (drawing == value)
                    return;
                drawing = value;
                OnPropertyChanged();
            }
        }

        public double PointSize
        {
            get { return pointSize; }
            set
            {
                if (pointSize == value)
                    return;
                pointSize = value;
                OnPropertyChanged();
            }
        }

        public ObservableHashSet<Entity> SelectedEntities
        {
            get { return selectedEntities; }
            set
            {
                if (selectedEntities == value)
                    return;
                selectedEntities = value;
                OnPropertyChanged();
            }
        }

        public ColorMap ColorMap
        {
            get { return colorMap; }
            set
            {
                if (colorMap == value)
                    return;
                colorMap = value;
                OnPropertyChanged();
            }
        }

        public Point CursorPoint
        {
            get { return cursorPoint; }
            set
            {
                if (cursorPoint == value)
                    return;
                cursorPoint = value;
                OnPropertyChanged();
            }
        }

        public RubberBandGenerator RubberBandGenerator
        {
            get { return rubberBandGenerator; }
            set
            {
                if (rubberBandGenerator == value)
                    return;
                rubberBandGenerator = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
