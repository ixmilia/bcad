using BCad.Commands;
using BCad.Core.UI;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    [Export(typeof(IUIWorkspace))]
    [Shared]
    internal class Workspace : WorkspaceBase, IUIWorkspace
    {
        private const string SettingsFile = "BCad.settings.xml";

        private string FullSettingsFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFile); }
        }

        public Workspace()
        {
            Update(drawing: Drawing.Update(author: Environment.UserName));

            // supliment non-UI commands
            SupplimentedCommands = new List<CommandSuppliment>()
            {
                new CommandSuppliment("Debug.Dump", ModifierKeys.None, Key.None, "dump"),
                new CommandSuppliment("Edit.Layers", ModifierKeys.Control, Key.L, "layers", "layer", "la"),
                new CommandSuppliment("Edit.Redo", ModifierKeys.Control, Key.Y, "redo", "re", "r"),
                new CommandSuppliment("Edit.Undo", ModifierKeys.Control, Key.Z, "undo", "u"),
                new CommandSuppliment("File.New", ModifierKeys.Control, Key.N, "new", "n"),
                new CommandSuppliment("File.Open", ModifierKeys.Control, Key.O, "open", "o"),
                new CommandSuppliment("File.Plot", ModifierKeys.Control, Key.P, "plot"),
                new CommandSuppliment("File.SaveAs", ModifierKeys.None, Key.None, "saveas", "sa"),
                new CommandSuppliment("File.Save", ModifierKeys.Control, Key.S, "save", "s"),
                new CommandSuppliment("View.ZoomExtents", ModifierKeys.None, Key.None, "zoomextents", "ze"),
                new CommandSuppliment("View.ZoomWindow", ModifierKeys.None, Key.None, "zoomwindow", "zw")
            };
        }

        [ImportMany]
        public IEnumerable<Lazy<IUICommand, UICommandMetadata>> UICommands { get; set; }

        public IEnumerable<CommandSuppliment> SupplimentedCommands { get; private set; }

        protected override ISettingsManager LoadSettings()
        {
            SettingsManager manager = null;
            if (File.Exists(FullSettingsFile))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(SettingsManager));
                    using (var stream = new FileStream(FullSettingsFile, FileMode.Open))
                    {
                        manager = (SettingsManager)serializer.Deserialize(stream);
                    }
                }
                catch
                {
                }
            }

            return manager ?? new SettingsManager();
        }

        public override void SaveSettings()
        {
            var serializer = new XmlSerializer(typeof(SettingsManager));
            using (var stream = new FileStream(FullSettingsFile, FileMode.Create))
            {
                serializer.Serialize(stream, this.SettingsManager);
            }
        }

        public override async Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            var result = UnsavedChangesResult.Discarded;
            if (this.IsDirty)
            {
                string filename = Drawing.Settings.FileName ?? "(Untitled)";
                var dialog = MessageBox.Show(string.Format("Save changes to '{0}'?", filename),
                    "Unsaved changes",
                    MessageBoxButton.YesNoCancel);
                switch (dialog)
                {
                    case MessageBoxResult.Yes:
                        var fileName = Drawing.Settings.FileName;
                        if (fileName == null)
                            fileName = await FileSystemService.GetFileNameFromUserForSave();
                        if (fileName == null)
                            result = UnsavedChangesResult.Cancel;
                        else if (await FileSystemService.TryWriteDrawing(fileName, Drawing, ActiveViewPort, null))
                            result = UnsavedChangesResult.Saved;
                        else
                            result = UnsavedChangesResult.Cancel;
                        break;
                    case MessageBoxResult.No:
                        result = UnsavedChangesResult.Discarded;
                        break;
                    case MessageBoxResult.Cancel:
                        result = UnsavedChangesResult.Cancel;
                        break;
                }
            }
            else
            {
                result = UnsavedChangesResult.Saved;
            }

            return result;
        }

        protected override Tuple<Commands.ICommand, string> GetCommand(string commandName)
        {
            // first look for a supplimented command
            string realCommandName = null;
            var realCommand = SupplimentedCommands.SingleOrDefault(c => c.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase));
            if (realCommand != null)
            {
                realCommandName = realCommand.Name;
            }

            // otherwise do a normal search
            var command = base.GetCommand(realCommandName ?? commandName);
            if (command == null)
            {
                var lazyCommand = (from c in UICommands
                                   let data = c.Metadata
                                   where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                                      || data.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase)
                                   select c).SingleOrDefault();
                command = lazyCommand == null ? null : Tuple.Create((Commands.ICommand)lazyCommand.Value, lazyCommand.Metadata.DisplayName);
            }

            return command;
        }
    }
}
