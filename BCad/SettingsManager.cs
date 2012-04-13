using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using BCad.EventArguments;

namespace BCad
{
    [Serializable]
    public class SettingsManager : ISettingsManager
    {
        private string layerDialogId = null;
        private string viewControlId = null;
        private string consoleControlId = null;
        private bool orthoganalLines = false;

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

        public bool OrthoganalLines
        {
            get { return this.orthoganalLines; }
            set
            {
                if (this.orthoganalLines == value)
                    return;
                this.orthoganalLines = value;
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
            OrthoganalLines = false;
        }
    }
}
