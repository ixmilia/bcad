namespace IxMilia.BCad.Commands
{
    public struct CommandShortcut
    {
        public string Name { get; }
        public ModifierKeys ModifierKeys { get; }
        public Key Key { get; }

        public CommandShortcut(string name, ModifierKeys modifierKeys, Key key)
        {
            Name = name;
            ModifierKeys = modifierKeys;
            Key = key;
        }
    }
}
