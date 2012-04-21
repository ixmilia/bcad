using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BCad.Commands;
using BCad.EventArguments;
using BCad.UI;
using System.Diagnostics;

namespace BCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPartImportsSatisfiedNotification
    {
        public MainWindow()
        {
            InitializeComponent();

            App.Container.SatisfyImportsOnce(this);
        }

        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IInputService InputService = null;

        [Import]
        private IView View = null;

        [ImportMany]
        private IEnumerable<Lazy<ToolBar>> ToolBars = null;

        [ImportMany]
        private IEnumerable<Lazy<ViewControl, IViewControlMetadata>> Views = null;

        [ImportMany]
        private IEnumerable<Lazy<ConsoleControl, IConsoleMetadata>> Consoles = null;

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        private string ConfigFile
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return "BCad.config";
                }
                else
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BCad.config");
                }
            }
        }

        public void OnImportsSatisfied()
        {
            Workspace.LoadSettings(ConfigFile);
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.CurrentLayerChanged += Workspace_CurrentLayerChanged;
            Workspace.DocumentChanged += Workspace_DocumentChanged;

            // add keyboard shortcuts for command bindings
            foreach (var command in from c in Commands
                                    where c.Metadata.Key != Key.None
                                       || c.Metadata.Modifier != ModifierKeys.None
                                    select c.Metadata)
            {
                this.InputBindings.Add(new InputBinding(
                    new UserCommand(this.Workspace, command.Name),
                    new KeyGesture(command.Key, command.Modifier)));
            }

            // add keyboard shortcuts for toggled settings
            var settings = Workspace.SettingsManager;
            if (settings.AngleSnapShortcut.HasValue)
            {
                this.InputBindings.Add(new InputBinding(
                    new ToggleSettingsCommand(InputService, settings, "AngleSnap"),
                    new KeyGesture(settings.AngleSnapShortcut.Key, settings.AngleSnapShortcut.Modifier)));
            }
        }

        private void Workspace_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            TakeFocus();
        }

        private void Workspace_CurrentLayerChanged(object sender, LayerChangedEventArgs e)
        {
            TakeFocus();
        }

        private void TakeFocus()
        {
            this.Dispatcher.BeginInvoke((Action)(() => FocusHelper.Focus(this.inputPanel.Content as UserControl)));
        }

        void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            TakeFocus();
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var toolbar in ToolBars)
            {
                this.toolbarTray.ToolBars.Add(toolbar.Value);
            }

            var console = Consoles.First(c => c.Metadata.ControlId == Workspace.SettingsManager.ConsoleControlId).Value;
            this.inputPanel.Content = console;
            TakeFocus();
            InputService.Reset();

            var view = Views.First(v => v.Metadata.ControlId == Workspace.SettingsManager.ViewControlId).Value;
            this.viewPanel.Content = view;
            View.RegisteredControl = view;

            View.UpdateView(viewPoint: new Point(0, 0, 1),
                sight: Vector.ZAxis,
                up: Vector.YAxis,
                viewWidth: 300.0,
                bottomLeft: new Point(-10, -10, 0));

            SetTitle(Workspace.Document);
        }

        private void SetTitle(Document document)
        {
            string filename = document.FileName == null ? "(Untitled)" : Path.GetFileName(document.FileName);
            Dispatcher.BeginInvoke((Action)(() =>
                this.Title = string.Format("BCad [{0}]{1}", filename, document.IsDirty ? " *" : "")));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            Workspace.SaveSettings(ConfigFile);
        }
    }

    static class FocusHelper
    {
        private delegate void MethodInvoker();

        public static void Focus(UIElement element)
        {
            //Focus in a callback to run on another thread, ensuring the main UI thread is initialized by the
            //time focus is set
            ThreadPool.QueueUserWorkItem(delegate(Object foo)
            {
                UIElement elem = (UIElement)foo;
                elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (MethodInvoker)delegate()
                    {
                        elem.Focus();
                        Keyboard.Focus(elem);
                    });
            }, element);
        }
    }
}
