using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using Media = System.Windows.Media;

namespace BCad
{
    [Serializable]
    public class SettingsManager : ISettingsManager
    {
        private string layerDialogId = null;
        private string viewControlId = null;
        private string consoleControlId = null;
        private double snapPointDist = 0.0;
        private double snapPointSize = 0.0;
        private double objectSelectionRadius = 0.0;
        private bool pointSnap = false;
        private bool angleSnap = false;
        private bool ortho = false;
        private double snapAngleDist = 0.0;
        private double[] snapAngles = null;
        private KeyboardShortcut angleSnapShortcut = null;
        private KeyboardShortcut pointSnapShortcut = null;
        private KeyboardShortcut orthoShortcut = null;
        private Media.Color backgroundColor = Media.Colors.Black;

        internal IInputService InputService { get; set; }

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

        public string ViewControlId
        {
            get { return this.viewControlId; }
            set
            {
                if (this.viewControlId == value)
                    return;
                this.viewControlId = value;
                OnPropertyChanged("ViewControlId");
            }
        }

        public string ConsoleControlId
        {
            get { return this.consoleControlId; }
            set
            {
                if (this.consoleControlId == value)
                    return;
                this.consoleControlId = value;
                OnPropertyChanged("ConsoleControlId");
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

        public double ObjectSelectionRadius
        {
            get { return this.objectSelectionRadius; }
            set
            {
                if (this.objectSelectionRadius == value)
                    return;
                this.objectSelectionRadius = value;
                OnPropertyChanged("ObjectSelectionRadius");
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
                OnPropertyChanged("PointSnap");
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
                OnPropertyChanged("AngleSnap");
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
                OnPropertyChanged("Ortho");
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
        [XmlElement(ElementName="SnapAngles")]
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

        public KeyboardShortcut AngleSnapShortcut
        {
            get { return this.angleSnapShortcut; }
            set
            {
                this.angleSnapShortcut = value;
                OnPropertyChanged("AngleSnapShortcut");
            }
        }

        public KeyboardShortcut PointSnapShortcut
        {
            get { return this.pointSnapShortcut; }
            set
            {
                this.pointSnapShortcut = value;
                OnPropertyChanged("PointSnapShortcut");
            }
        }

        public KeyboardShortcut OrthoShortcut
        {
            get { return this.orthoShortcut; }
            set
            {
                this.orthoShortcut = value;
                OnPropertyChanged("OrthoShortcut");
            }
        }

        [XmlIgnore]
        public Media.Color BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                if (this.backgroundColor == value)
                    return;
                this.backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(ElementName="BackgroundColor")]
        public string BackgroundColorString
        {
            get
            {
                int c = (BackgroundColor.R << 16) | (BackgroundColor.G << 8) | BackgroundColor.B;
                return string.Format("#{0:X}", c);
            }
            set
            {
                int c = int.Parse(value.Substring(1), NumberStyles.HexNumber);
                int r = (c & 0xFF0000) >> 16;
                int g = (c & 0x00FF00) >> 8;
                int b = (c & 0x0000FF);
                BackgroundColor = Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
            }
        }

        public SettingsManager()
        {
            LoadDefaults();

            //if (File.Exists(fileName))
            //{
            //    var xml = XDocument.Load(fileName).Root;
            //    SetValue(xml, "LayerDialogId", ref layerDialogId);
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            // if the property is a simple boolean
            var info = this.GetType().GetProperty(propertyName);
            if (info != null && info.PropertyType == typeof(bool))
            {
                WriteLine("{0} is {1}", propertyName, (bool)GetValue(propertyName) ? "on" : "off");
            }

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WriteLine(string text, params object[] args)
        {
            if (InputService != null)
                InputService.WriteLine(text, args);
        }

        private object GetValue(string propertyName)
        {
            var prop = this.GetType().GetProperty(propertyName);
            return prop.GetValue(this, null);
        }

        //private static void SetValue(XElement xml, string elementName, ref string container)
        //{
        //    var element = xml.Element(elementName);
        //    if (element != null)
        //        container = element.Value;
        //}

        private void LoadDefaults()
        {
            LayerDialogId = "Default";
            ViewControlId = "Default";
            ConsoleControlId = "Default";
            SnapPointDistance = 15.0;
            SnapPointSize = 15.0;
            ObjectSelectionRadius = 2.0;
            PointSnap = true;
            AngleSnap = true;
            Ortho = false;
            SnapAngleDistance = 30.0;
            SnapAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
            AngleSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F7);
            PointSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F3);
            OrthoShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F8);
            BackgroundColor = Media.Colors.DarkSlateGray;
            //BackgroundColor = 0x6495ED; // cornflower blue
        }
    }
}
