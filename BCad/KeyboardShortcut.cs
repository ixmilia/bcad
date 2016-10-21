using System.Windows.Input;
using IxMilia.Config;

namespace BCad
{
    public class KeyboardShortcut
    {
        public ModifierKeys Modifier { get; set; }
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

        public override string ToString()
        {
            return string.Concat(Modifier.ToConfigString(), "+", Key.ToConfigString());
        }

        public static KeyboardShortcut Parse(string s)
        {
            var parts = s.Split('+');
            ModifierKeys modifier;
            Key key;
            parts[0].TryParseValue(out modifier);
            parts[1].TryParseValue(out key);
            return new KeyboardShortcut(modifier, key);
        }
    }
}
