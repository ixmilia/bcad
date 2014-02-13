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
        private const string ConfigFile = "BCad.config.xml";

        private string FullConfigFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigFile); }
        }

        public Workspace()
        {
            Update(drawing: Drawing.Update(author: Environment.UserName));
        }

        protected override ISettingsManager LoadSettings()
        {
            SettingsManager manager = null;
            if (File.Exists(FullConfigFile))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(SettingsManager));
                    using (var stream = new FileStream(FullConfigFile, FileMode.Open))
                    {
                        manager = (SettingsManager)serializer.Deserialize(stream);
                        manager.SetInputService(InputService);
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
            using (var stream = new FileStream(FullConfigFile, FileMode.Create))
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
                        else if (await FileSystemService.TryWriteDrawing(fileName, Drawing, ActiveViewPort))
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

        //public override void Update(Drawing drawing = null, Plane drawingPlane = null, ViewPort activeViewPort = null, IViewControl viewControl = null, bool? isDirty = true)
        //{
        //    if (activeViewPort != null && drawing == null && drawingPlane == null && viewControl == null && isDirty == true)
        //    {
        //        // only the viewport is changing
        //        if (ActiveViewPort.Sight == activeViewPort.Sight &&
        //            ActiveViewPort.Up == activeViewPort.Up &&
        //            ActiveViewPort.ViewHeight != activeViewPort.ViewHeight)
        //        {
        //            // keeping vectors, only animate if view height is changing (if only bottom left, it's a pan operation)
        //            var stepCount = 10;
        //            var stepDuration = TimeSpan.FromMilliseconds(15);
        //            var cornerDelta = (activeViewPort.BottomLeft - ActiveViewPort.BottomLeft) / (stepCount + 1);
        //            var heightDelta = (activeViewPort.ViewHeight - ActiveViewPort.ViewHeight) / (stepCount + 1);
        //            Task.Factory.StartNew(() =>
        //                {
        //                    var currentVp = ActiveViewPort.Update(bottomLeft: ActiveViewPort.BottomLeft + cornerDelta, viewHeight: ActiveViewPort.ViewHeight + heightDelta);
        //                    base.Update(activeViewPort: currentVp);
        //                    for (int i = 0; i < stepCount - 1; i++)
        //                    {
        //                        Thread.Sleep(stepDuration);
        //                        currentVp = currentVp.Update(bottomLeft: currentVp.BottomLeft + cornerDelta, viewHeight: currentVp.ViewHeight + heightDelta);
        //                        base.Update(activeViewPort: currentVp);
        //                    }

        //                    // do final update
        //                    Thread.Sleep(stepDuration);
        //                    base.Update(drawing, drawingPlane, activeViewPort, viewControl, isDirty);
        //                });
        //            return;
        //        }
        //    }
            
        //    // otherwise, do normal processing
        //    base.Update(drawing, drawingPlane, activeViewPort, viewControl, isDirty);
        //}
    }
}
