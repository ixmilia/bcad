using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using BCad.Extensions;
using BCad.SnapPoints;

namespace BCad
{
    public class DefaultSettingsManager : ISettingsManager
    {
        private string[] _ribbonOrder = null;
        private string _layerDialogId = null;
        private string _plotDialogId = null;
        private string _rendererId = null;
        private double _snapPointDist = 0.0;
        private double _snapPointSize = 0.0;
        private double _pointSize = 0.0;
        private double _entitySelectionRadius = 0.0;
        private int _cursorSize = 0;
        private int _textCursorSize = 0;
        private bool _pointSnap = false;
        private bool _angleSnap = false;
        private bool _ortho = false;
        private bool _debug = false;
        private double _snapAngleDist = 0.0;
        private double[] _snapAngles = null;
        private CadColor _backgroundColor = CadColor.Black;
        private CadColor _snapPointColor = CadColor.Yellow;
        private CadColor _hotPointColor = CadColor.Blue;
        private SnapPointKind _allowedSnapPoints = SnapPointKind.All;

        [XmlIgnore]
        public string[] RibbonOrder
        {
            get { return _ribbonOrder; }
            set
            {
                _ribbonOrder = value;
                OnPropertyChanged(nameof(RibbonOrder));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(RibbonOrder))]
        public string RibbonOrderString
        {
            get
            {
                return string.Join(";", _ribbonOrder);
            }
            set
            {
                _ribbonOrder = value.Split(';').ToArray();
            }
        }

        public string LayerDialogId
        {
            get { return _layerDialogId; }
            set
            {
                if (_layerDialogId == value)
                    return;
                _layerDialogId = value;
                OnPropertyChanged(nameof(LayerDialogId));
            }
        }

        public string PlotDialogId
        {
            get { return _plotDialogId; }
            set
            {
                if (_plotDialogId == value)
                    return;
                _plotDialogId = value;
                OnPropertyChanged(nameof(PlotDialogId));
            }
        }

        public string RendererId
        {
            get { return _rendererId; }
            set
            {
                if (_rendererId == value)
                    return;
                _rendererId = value;
                OnPropertyChanged(nameof(RendererId));
            }
        }

        public double SnapPointDistance
        {
            get { return _snapPointDist; }
            set
            {
                if (_snapPointDist == value)
                    return;
                _snapPointDist = value;
                OnPropertyChanged(nameof(SnapPointDistance));
            }
        }

        public double SnapPointSize
        {
            get { return _snapPointSize; }
            set
            {
                if (_snapPointSize == value)
                    return;
                _snapPointSize = value;
                OnPropertyChanged(nameof(SnapPointSize));
            }
        }

        public double PointSize
        {
            get { return _pointSize; }
            set
            {
                if (_pointSize == value)
                    return;
                _pointSize = value;
                OnPropertyChanged(nameof(PointSize));
            }
        }

        public double EntitySelectionRadius
        {
            get { return _entitySelectionRadius; }
            set
            {
                if (_entitySelectionRadius == value)
                    return;
                _entitySelectionRadius = value;
                OnPropertyChanged(nameof(EntitySelectionRadius));
            }
        }

        public int CursorSize
        {
            get { return _cursorSize; }
            set
            {
                if (_cursorSize == value)
                    return;
                _cursorSize = value;
                OnPropertyChanged(nameof(CursorSize));
            }
        }

        public int TextCursorSize
        {
            get { return _textCursorSize; }
            set
            {
                if (_textCursorSize == value)
                    return;
                _textCursorSize = value;
                OnPropertyChanged(nameof(TextCursorSize));
            }
        }

        public bool PointSnap
        {
            get { return _pointSnap; }
            set
            {
                if (_pointSnap == value)
                    return;
                _pointSnap = value;
                OnPropertyChanged(nameof(PointSnap));
            }
        }

        public bool AngleSnap
        {
            get { return _angleSnap; }
            set
            {
                if (_angleSnap == value)
                    return;
                _angleSnap = value;
                OnPropertyChanged(nameof(AngleSnap));
            }
        }

        public bool Ortho
        {
            get { return _ortho; }
            set
            {
                if (_ortho == value)
                    return;
                _ortho = value;
                OnPropertyChanged(nameof(Ortho));
            }
        }

        public bool Debug
        {
            get { return _debug; }
            set
            {
                if (_debug == value)
                    return;
                _debug = value;
                OnPropertyChanged(nameof(Debug));
            }
        }

        public double SnapAngleDistance
        {
            get { return _snapAngleDist; }
            set
            {
                if (_snapAngleDist == value)
                    return;
                _snapAngleDist = value;
                OnPropertyChanged(nameof(SnapAngleDistance));
            }
        }

        [XmlIgnore]
        public double[] SnapAngles
        {
            get { return _snapAngles; }
            set
            {
                _snapAngles = value;
                OnPropertyChanged(nameof(SnapAngles));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(SnapAngles))]
        public string SnapAnglesString
        {
            get
            {
                return string.Join(";", SnapAngles);
            }
            set
            {
                SnapAngles = value.Split(';').Select(s => double.Parse(s.Trim())).ToArray();
            }
        }

        [XmlIgnore]
        public CadColor BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor == value)
                    return;
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(BackgroundColor))]
        public string BackgroundColorString
        {
            get
            {
                return BackgroundColor.ToColorString();
            }
            set
            {
                BackgroundColor = value.ParseColor();
            }
        }

        [XmlIgnore]
        public CadColor SnapPointColor
        {
            get { return _snapPointColor; }
            set
            {
                if (_snapPointColor == value)
                    return;
                _snapPointColor = value;
                OnPropertyChanged(nameof(SnapPointColor));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(SnapPointColor))]
        public string SnapPointColorString
        {
            get { return SnapPointColor.ToColorString(); }
            set { SnapPointColor = value.ParseColor(); }
        }

        [XmlIgnore]
        public CadColor HotPointColor
        {
            get { return _hotPointColor; }
            set
            {
                if (_hotPointColor == value)
                    return;
                _hotPointColor = value;
                OnPropertyChanged(nameof(HotPointColor));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(HotPointColor))]
        public string HotPointColorString
        {
            get { return HotPointColor.ToColorString(); }
            set { HotPointColor = value.ParseColor(); }
        }

        [XmlIgnore]
        public SnapPointKind AllowedSnapPoints
        {
            get { return _allowedSnapPoints; }
            set
            {
                _allowedSnapPoints = value;
                OnPropertyChanged(nameof(AllowedSnapPoints));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = nameof(AllowedSnapPoints))]
        public string AllowedSnapPointsString
        {
            get { return AllowedSnapPoints.ToString(); }
            set
            {
                AllowedSnapPoints = (SnapPointKind)Enum.Parse(typeof(SnapPointKind), value);
            }
        }

        public DefaultSettingsManager()
        {
            LoadDefaults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void LoadDefaults()
        {
            RibbonOrder = new[] { "home", "view", "settings", "debug" };
            LayerDialogId = "Default";
            PlotDialogId = "Default";
            RendererId = "Hardware";
            SnapPointDistance = 15.0;
            SnapPointSize = 15.0;
            PointSize = 15.0;
            EntitySelectionRadius = 3.0;
            CursorSize = 60;
            TextCursorSize = 18;
            PointSnap = true;
            AngleSnap = true;
            Ortho = false;
            Debug = false;
            SnapAngleDistance = 30.0;
            SnapAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
            BackgroundColor = CadColor.DarkSlateGray;
            SnapPointColor = CadColor.Yellow;
            HotPointColor = CadColor.Blue;
            AllowedSnapPoints = SnapPointKind.All;
        }
    }
}
