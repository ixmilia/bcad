using Input = System.Windows.Input;

namespace BCad.Core.UI.Extensions
{
    public static class KeyExtensions
    {
        public static Input.Key ToInputKey(this Key key)
        {
            return (Input.Key)((int)key);
        }

        public static Input.ModifierKeys ToInputModifierKeys(this ModifierKeys modifierKeys)
        {
            return (Input.ModifierKeys)((int)modifierKeys);
        }
    }
}
