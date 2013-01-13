using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using BCad.Collections;
using BCad.Commands;
using BCad.Entities;
using BCad.EventArguments;
using BCad.FileHandlers;
using BCad.Services;
using BCad.UI;
using BCad.Helpers;

namespace BCad
{
    [Export(typeof(IWorkspace))]
    internal class Workspace : IWorkspace
    {
        public Workspace()
        {
            Drawing = new Drawing();
            DrawingPlane = new Plane(Point.Origin, Vector.ZAxis);
            ActiveViewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
            SelectedEntities = new ObservableHashSet<Entity>();
            ViewControl = null;

            LoadSettings(ConfigFile);
        }

        #region Events

        public event CommandExecutingEventHandler CommandExecuting;

        protected virtual void OnCommandExecuting(CommandExecutingEventArgs e)
        {
            if (CommandExecuting != null)
                CommandExecuting(this, e);
        }

        public event CommandExecutedEventHandler CommandExecuted;

        protected virtual void OnCommandExecuted(CommandExecutedEventArgs e)
        {
            if (CommandExecuted != null)
                CommandExecuted(this, e);
        }

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public Drawing Drawing { get; private set; }

        public Plane DrawingPlane { get; private set; }

        public ViewPort ActiveViewPort { get; private set; }

        public ViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

        #endregion

        #region Imports

        [Import]
        private IInputService InputService = null;

        // required so that the service is created before the undo or redo commands are fired
        [Import]
        private IUndoRedoService UndoRedoService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        [ImportMany]
        private IEnumerable<IFileWriter> FileWriters = null;

        #endregion

        #region IWorkspace implementation

        public ISettingsManager SettingsManager { get; private set; }

        public void Update(
            Drawing drawing = null,
            Plane drawingPlane = null,
            ViewPort activeViewPort = null,
            ViewControl viewControl = null,
            bool? isDirty = true)
        {
            var e = new WorkspaceChangeEventArgs(
                drawing != null,
                drawingPlane != null,
                activeViewPort != null,
                viewControl != null,
                isDirty != null);

            OnWorkspaceChanging(e);
            if (drawing != null)
                this.Drawing = drawing;
            if (drawingPlane != null)
                this.DrawingPlane = drawingPlane;
            if (activeViewPort != null)
                this.ActiveViewPort = activeViewPort;
            if (viewControl != null)
                this.ViewControl = viewControl;
            if (isDirty != null)
                this.IsDirty = isDirty.Value;
            OnWorkspaceChanged(e);
        }

        public event WorkspaceChangingEventHandler WorkspaceChanging;

        protected void OnWorkspaceChanging(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanging;
            if (handler != null)
                handler(this, e);
        }

        public event WorkspaceChangedEventHandler WorkspaceChanged;

        protected void OnWorkspaceChanged(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanged;
            if (handler != null)
                handler(this, e);
        }

        private void LoadSettings(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(SettingsManager));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var manager = (SettingsManager)serializer.Deserialize(stream);
                        manager.InputService = this.InputService;
                        this.SettingsManager = manager;
                    }
                }
                catch
                {
                    this.SettingsManager = new SettingsManager();
                }
            }
            else
            {
                this.SettingsManager = new SettingsManager();
            }
        }

        public void SaveSettings()
        {
            var serializer = new XmlSerializer(typeof(SettingsManager));
            using (var stream = new FileStream(ConfigFile, FileMode.Create))
            {
                serializer.Serialize(stream, this.SettingsManager);
            }
        }

        private bool Execute(BCad.Commands.ICommand command, object arg)
        {
            OnCommandExecuting(new CommandExecutingEventArgs(command));
            InputService.WriteLine(command.DisplayName);
            bool result = command.Execute(arg);
            OnCommandExecuted(new CommandExecutedEventArgs(command));
            return result;
        }

        public bool ExecuteCommandSynchronous(string commandName, object arg)
        {
            if (commandName == null && lastCommand == null)
            {
                return false;
            }

            lock (executeGate)
            {
                if (isExecuting)
                    return false;
                isExecuting = true;
            }

            commandName = commandName ?? lastCommand;
            var command = GetCommand(commandName);
            if (command == null)
            {
                InputService.WriteLine("Command {0} not found", commandName);
                isExecuting = false;
                return false;
            }

            bool result = Execute(command, arg);
            lastCommand = commandName;
            lock (executeGate)
            {
                isExecuting = false;
            }

            return result;
        }

        public void ExecuteCommand(string commandName, object arg)
        {
            ThreadPool.QueueUserWorkItem(_ => ExecuteCommandSynchronous(commandName, arg));
        }

        public bool CommandExists(string commandName)
        {
            return GetCommand(commandName) != null;
        }

        public bool CanExecute()
        {
            return !this.isExecuting;
        }

        public UnsavedChangesResult PromptForUnsavedChanges()
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
                        // TODO: can't execute another command
                        if (SaveAsCommand.Execute(this, FileWriters, Drawing.Settings.FileName))
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

        public void Focus()
        {
            Application.Current.Dispatcher.Invoke((Action)(() => Application.Current.MainWindow.Focus()));
        }

        public string FormatUnits(double value)
        {
            var feet = (int)value / 12;
            var inches = (int)value % 12;

            // TODO: add 0.5 * 1/16 to simulate rounding?
            var frac = (value - ((double)(int)value)) + MathHelper.Epsilon;
            int fracPart;
            for (fracPart = 0; fracPart < unitSixteenths.Length - 1; fracPart++)
            {
                if (frac >= unitSixteenths[fracPart] && frac < unitSixteenths[fracPart + 1])
                {
                    break;
                }
            }

            return string.Format(@"{0}'-{1}""-{2}/{3}", feet, inches, fracPart, 16);
        }

        static double[] unitSixteenths = new double[]
        {
            0.0,    //  0/16
            0.0625, //  1/16
            0.125,  //  2/16 - 1/8
            0.1875, //  3/16
            0.25,   //  4/16 - 1/4
            0.3125, //  5/16
            0.375,  //  6/16 - 3/8
            0.4375, //  7/16
            0.5,    //  8/16 - 1/2
            0.5625, //  9/16
            0.625,  // 10/16 - 5/8
            0.6875, // 11/16
            0.75,   // 12/16 - 3/4
            0.8125, // 13/16
            0.875,  // 14/16 - 7/8
            0.9375, // 15/16
        };

        #endregion

        #region Privates

        private bool isExecuting = false;
        private string lastCommand = null;
        private object executeGate = new object();

        private BCad.Commands.ICommand GetCommand(string commandName)
        {
            var command = (from c in Commands
                           let data = c.Metadata
                           where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                              || data.CommandAliases.Contains(commandName, StringComparer.OrdinalIgnoreCase)
                           select c).SingleOrDefault();
            return command == null ? null : command.Value;
        }

        private const string ConfigFile = "BCad.config";

        #endregion

    }
}
