using System;
using System.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BCad
{
    [Export(typeof(IWorkspace)), Shared]
    internal class Workspace : WorkspaceBase
    {
        private const string SettingsFile = "BCad.settings.xml";

        private string FullSettingsFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFile); }
        }

        public Workspace()
        {
            Update(drawing: Drawing.Update(author: Environment.UserName));
        }

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
                    manager = new SettingsManager();
                }
            }

            return manager;
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
    }
}
