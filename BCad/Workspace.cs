using System;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using BCad.Services;

namespace BCad
{
    [Export(typeof(IWorkspace)), Shared]
    internal class Workspace : WorkspaceBase
    {
        private const string SettingsFile = "BCad.settings.xml";
        private Regex SettingsPattern = new Regex(@"^/p:([a-zA-Z]+)=(.*)$");

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
            var serializer = new XmlSerializer(typeof(SettingsManager));
            SettingsManager manager = null;
            if (File.Exists(FullSettingsFile))
            {
                try
                {
                    using (var stream = new FileStream(FullSettingsFile, FileMode.Open))
                    {
                        manager = (SettingsManager)serializer.Deserialize(stream);
                    }
                }
                catch
                {
                }
            }

            if (manager == null)
            {
                manager = new SettingsManager();
            }

            // Override settings provided via the command line in the form of "/p:SettingName=SettingValue".  To do this
            // we need to serialize the settings, replace the specified values, then deserialize again.
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                // serialize the settings manager back to xml
                var ms = new MemoryStream();
                serializer.Serialize(ms, manager);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var xml = XDocument.Load(ms);

                // set each value as specified on the command line
                foreach (var argument in args.Skip(1))
                {
                    var match = SettingsPattern.Match(argument);
                    if (match.Success)
                    {
                        var settingName = match.Groups[1].Value;
                        var settingValue = match.Groups[2].Value;
                        var element = xml.Root.Element(settingName);
                        if (element != null)
                        {
                            element.Value = settingValue;
                        }
                    }
                }

                // now deserialize again
                try
                {
                    using (var reader = new StringReader(xml.ToString()))
                    {
                        manager = (SettingsManager)serializer.Deserialize(reader);
                    }
                }
                catch
                {
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
            var fileSystemService = GetService<IFileSystemService>();
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
                            fileName = await fileSystemService.GetFileNameFromUserForSave();
                        if (fileName == null)
                            result = UnsavedChangesResult.Cancel;
                        else if (await fileSystemService.TryWriteDrawing(fileName, Drawing, ActiveViewPort))
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
