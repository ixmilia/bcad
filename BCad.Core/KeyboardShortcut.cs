using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;

namespace BCad
{
    public class KeyboardShortcut
    {
        [XmlAttribute]
        public ModifierKeys Modifier { get; set; }

        [XmlAttribute]
        public Key Key { get; set; }

        public KeyboardShortcut()
            : this(ModifierKeys.None, Key.None)
        {
        }

        public KeyboardShortcut(ModifierKeys modifier, Key key)
        {
            this.Modifier = modifier;
            this.Key = key;
        }

        public bool HasValue
        {
            get
            {
                return this.Modifier != ModifierKeys.None || this.Key != Key.None;
            }
        }
    }
}
