using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using BCad.Extensions;
using BCad.Services;
using BCad.SnapPoints;

namespace BCad
{
    public class DefaultSettingsManager : ISettingsManager
    {
        private string[] ribbonOrder = null;
        private string layerDialogId = null;
        private string plotDialogId = null;
        private string rendererId = null;
        private double snapPointDist = 0.0;
        private double snapPointSize = 0.0;
        private double entitySelectionRadius = 0.0;
        private int cursorSize = 0;
        private int textCursorSize = 0;
        private bool pointSnap = false;
        private bool angleSnap = false;
        private bool ortho = false;
        private bool debug = false;
        private double snapAngleDist = 0.0;
        private double[] snapAngles = null;
        private RealColor backgroundColor = RealColor.Black;
        private RealColor snapPointColor = RealColor.Yellow;
        private RealColor hotPointColor = RealColor.Blue;
        private ColorMap colorMap = null;
        private SnapPointKind allowedSnapPoints = SnapPointKind.All;

        [XmlIgnore]
        public string[] RibbonOrder
        {
            get { return ribbonOrder; }
            set
            {
                ribbonOrder = value;
                OnPropertyChanged("RibbonOrder");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "RibbonOrder")]
        public string RibbonOrderString
        {
            get
            {
                return string.Join(";", ribbonOrder);
            }
            set
            {
                ribbonOrder = value.Split(';').ToArray();
            }
        }

        public string LayerDialogId
        {
            get { return this.layerDialogId; }
            set
            {
                if (this.layerDialogId == value)
                    return;
                this.layerDialogId = value;
                OnPropertyChanged("LayerDialogId");
            }
        }

        public string PlotDialogId
        {
            get { return this.plotDialogId; }
            set
            {
                if (this.plotDialogId == value)
                    return;
                this.plotDialogId = value;
                OnPropertyChanged("PlotDialogId");
            }
        }

        public string RendererId
        {
            get { return this.rendererId; }
            set
            {
                if (this.rendererId == value)
                    return;
                this.rendererId = value;
                OnPropertyChanged("RendererId");
            }
        }

        public double SnapPointDistance
        {
            get { return this.snapPointDist; }
            set
            {
                if (this.snapPointDist == value)
                    return;
                this.snapPointDist = value;
                OnPropertyChanged("SnapPointDistance");
            }
        }

        public double SnapPointSize
        {
            get { return this.snapPointSize; }
            set
            {
                if (this.snapPointSize == value)
                    return;
                this.snapPointSize = value;
                OnPropertyChanged("SnapPointSize");
            }
        }

        public double EntitySelectionRadius
        {
            get { return this.entitySelectionRadius; }
            set
            {
                if (this.entitySelectionRadius == value)
                    return;
                this.entitySelectionRadius = value;
                OnPropertyChanged("EntitySelectionRadius");
            }
        }

        public int CursorSize
        {
            get { return this.cursorSize; }
            set
            {
                if (this.cursorSize == value)
                    return;
                this.cursorSize = value;
                OnPropertyChanged("CursorSize");
            }
        }

        public int TextCursorSize
        {
            get { return this.textCursorSize; }
            set
            {
                if (this.textCursorSize == value)
                    return;
                this.textCursorSize = value;
                OnPropertyChanged("TextCursorSize");
            }
        }

        public bool PointSnap
        {
            get { return this.pointSnap; }
            set
            {
                if (this.pointSnap == value)
                    return;
                this.pointSnap = value;
                OnPropertyChanged(Constants.PointSnapString);
            }
        }

        public bool AngleSnap
        {
            get { return this.angleSnap; }
            set
            {
                if (this.angleSnap == value)
                    return;
                this.angleSnap = value;
                OnPropertyChanged(Constants.AngleSnapString);
            }
        }

        public bool Ortho
        {
            get { return this.ortho; }
            set
            {
                if (this.ortho == value)
                    return;
                this.ortho = value;
                OnPropertyChanged(Constants.OrthoString);
            }
        }

        public bool Debug
        {
            get { return this.debug; }
            set
            {
                if (this.debug == value)
                    return;
                this.debug = value;
                OnPropertyChanged(Constants.DebugString);
            }
        }

        public double SnapAngleDistance
        {
            get { return this.snapAngleDist; }
            set
            {
                if (this.snapAngleDist == value)
                    return;
                this.snapAngleDist = value;
                OnPropertyChanged("SnapAngleDistance");
            }
        }

        [XmlIgnore]
        public double[] SnapAngles
        {
            get { return this.snapAngles; }
            set
            {
                this.snapAngles = value;
                OnPropertyChanged("SnapAngles");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "SnapAngles")]
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
        public RealColor BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                if (this.backgroundColor == value)
                    return;
                this.backgroundColor = value;
                OnPropertyChanged(Constants.BackgroundColorString);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = Constants.BackgroundColorString)]
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
        public RealColor SnapPointColor
        {
            get { return this.snapPointColor; }
            set
            {
                if (this.snapPointColor == value)
                    return;
                this.snapPointColor = value;
                OnPropertyChanged("SnapPointColor");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "SnapPointColor")]
        public string SnapPointColorString
        {
            get { return SnapPointColor.ToColorString(); }
            set { SnapPointColor = value.ParseColor(); }
        }

        [XmlIgnore]
        public RealColor HotPointColor
        {
            get { return this.hotPointColor; }
            set
            {
                if (this.hotPointColor == value)
                    return;
                this.hotPointColor = value;
                OnPropertyChanged("HotPointColor");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "HotPointColor")]
        public string HotPointColorString
        {
            get { return HotPointColor.ToColorString(); }
            set { HotPointColor = value.ParseColor(); }
        }

        [XmlIgnore]
        public ColorMap ColorMap
        {
            get { return colorMap; }
            set
            {
                colorMap = value;
                OnPropertyChanged("ColorMap");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "ColorMap")]
        public string ColorMapString
        {
            get
            {
                return colorMap.ToString();
            }
            set
            {
                colorMap = ColorMap.Parse(value);
            }
        }

        [XmlIgnore]
        public SnapPointKind AllowedSnapPoints
        {
            get { return allowedSnapPoints; }
            set
            {
                allowedSnapPoints = value;
                OnPropertyChanged("AllowedSnapPoints");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName = "AllowedSnapPoints")]
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
            EntitySelectionRadius = 3.0;
            CursorSize = 60;
            TextCursorSize = 18;
            PointSnap = true;
            AngleSnap = true;
            Ortho = false;
            Debug = false;
            SnapAngleDistance = 30.0;
            SnapAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
            BackgroundColor = RealColor.DarkSlateGray;
            SnapPointColor = RealColor.Yellow;
            HotPointColor = RealColor.Blue;
            ColorMap = ColorMap.Default;
            AllowedSnapPoints = SnapPointKind.All;
        }
    }
}
