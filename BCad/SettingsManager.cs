using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using BCad.EventArguments;
using System.Windows.Input;

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
        private bool angleSnap = false;
        private bool ortho = false;
        private double snapAngleDist = 0.0;
        private double[] snapAngles = null;
        private KeyboardShortcut angleSnapShortcut = null;
        private KeyboardShortcut orthoShortcut = null;

        public string LayerDialogId
        {
            get { return this.layerDialogId; }
            set
            {
                if (this.layerDialogId == value)
                    return;
                this.layerDialogId = value;
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
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
                OnSettingsChanged();
            }
        }

        public double[] SnapAngles
        {
            get { return this.snapAngles; }
            set
            {
                this.snapAngles = value;
                OnSettingsChanged();
            }
        }

        public KeyboardShortcut AngleSnapShortcut
        {
            get { return this.angleSnapShortcut; }
            set
            {
                this.angleSnapShortcut = value;
                OnSettingsChanged();
            }
        }

        public KeyboardShortcut OrthoShortcut
        {
            get { return this.orthoShortcut; }
            set
            {
                this.orthoShortcut = value;
                OnSettingsChanged();
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

        public event SettingsChangedEventHandler SettingsChanged;

        protected void OnSettingsChanged()
        {
            if (SettingsChanged != null)
                SettingsChanged(this, new SettingsChangedEventArgs(this));
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
            AngleSnap = true;
            Ortho = false;
            SnapAngleDistance = 15.0;
            SnapAngles = new[] { 0.0, 90.0, 180.0, 270.0 };
            AngleSnapShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F7);
            OrthoShortcut = new KeyboardShortcut(ModifierKeys.None, Key.F8);
        }
    }
}
