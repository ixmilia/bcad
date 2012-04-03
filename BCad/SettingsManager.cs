using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace BCad
{
    [Serializable]
    public class SettingsManager : ISettingsManager
    {
        public string LayerDialogId { get; set; }

        public SettingsManager()
        {
            //LoadDefaults();

            //if (File.Exists(fileName))
            //{
            //    var xml = XDocument.Load(fileName).Root;
            //    SetValue(xml, "LayerDialogId", ref layerDialogId);
            //}
        }

        //private static void SetValue(XElement xml, string elementName, ref string container)
        //{
        //    var element = xml.Element(elementName);
        //    if (element != null)
        //        container = element.Value;
        //}

        //public void LoadDefaults()
        //{
        //    layerDialogId = "Default";
        //}
    }
}
