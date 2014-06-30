using BCad.Commands;
using BCad.Core.UI;
using BCad.EventArguments;
using BCad.Primitives;
using BCad.Ribbons;
using BCad.Services;
using BCad.UI;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace BCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            App.Container.SatisfyImports(this);
        }

        [Import]
        public IUIWorkspace Workspace { get; set; }

        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<RibbonTab, RibbonTabMetadata>> RibbonTabs { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ConsoleControl, ConsoleMetadata>> Consoles { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<IUICommand, UICommandMetadata>> UICommands { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            // prepare status bar bindings
            foreach (var x in new[] { new { TextBlock = this.orthoStatus, Path = Constants.OrthoString },
                                      new { TextBlock = this.pointSnapStatus, Path = Constants.PointSnapString },
                                      new { TextBlock = this.angleSnapStatus, Path = Constants.AngleSnapString },
                                      new { TextBlock = this.debugStatus, Path = Constants.DebugString }})
            {
                var binding = new Binding(x.Path);
                binding.Source = Workspace.SettingsManager;
                binding.Converter = new BoolToBrushConverter();
                x.TextBlock.SetBinding(TextBlock.ForegroundProperty, binding);
            }

            // add keyboard shortcuts for command bindings
            foreach (var command in from c in UICommands
                                    let metadata = c.Metadata
                                    where metadata.Key != Key.None
                                       || metadata.Modifier != ModifierKeys.None
                                    select metadata)
            {
                this.InputBindings.Add(new InputBinding(
                    new UserCommand(this.Workspace, command.Name),
                    new KeyGesture(command.Key, command.Modifier)));
            }

            // add keyboard shortcuts for supplimented commands
            foreach (var command in from c in Workspace.SupplimentedCommands
                                    where c.Key != Key.None
                                       || c.Modifier != ModifierKeys.None
                                    select c)
            {
                this.InputBindings.Add(new InputBinding(
                    new UserCommand(this.Workspace, command.Name),
                    new KeyGesture(command.Key, command.Modifier)));
            }

            // add keyboard shortcuts for toggled settings
            var uiSettings = Workspace.SettingsManager as SettingsManager;
            Debug.Assert(uiSettings != null);
            foreach (var setting in new[] {
                new { Name = Constants.AngleSnapString, Shortcut = uiSettings.AngleSnapShortcut },
                new { Name = Constants.PointSnapString, Shortcut = uiSettings.PointSnapShortcut },
                new { Name = Constants.OrthoString, Shortcut = uiSettings.OrthoShortcut },
                new { Name = Constants.DebugString, Shortcut = uiSettings.DebugShortcut } })
            {
                if (setting.Shortcut.HasValue)
                {
                    this.InputBindings.Add(new InputBinding(
                        new ToggleSettingsCommand(Workspace.SettingsManager, setting.Name),
                        new KeyGesture(setting.Shortcut.Key, setting.Shortcut.Modifier)));
                }
            }

            Workspace_WorkspaceChanged(this, WorkspaceChangeEventArgs.Reset());
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.DebugString:
                    if (Workspace.SettingsManager.Debug)
                        SetDebugText();
                    else
                        this.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                debugText.Height = 0;
                                debugText.Text = "";
                            }));
                    break;
            }
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            SetTitle(Workspace.Drawing);
            if (e.IsDrawingChange)
            {
                TakeFocus();
                if (Workspace.SettingsManager.Debug)
                    SetDebugText();
            }
        }

        private void SetDebugText()
        {
            int lineCount = 0, ellipseCount = 0, pointCount = 0, textCount = 0;
            foreach (var ent in Workspace.Drawing.GetLayers().SelectMany(l => l.GetEntities()).SelectMany(en => en.GetPrimitives()))
            {
                switch (ent.Kind)
                {
                    case PrimitiveKind.Ellipse:
                        ellipseCount++;
                        break;
                    case PrimitiveKind.Line:
                        lineCount++;
                        break;
                    case PrimitiveKind.Point:
                        pointCount++;
                        break;
                    case PrimitiveKind.Text:
                        textCount++;
                        break;
                }
            }
            this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    debugText.Height = double.NaN;
                    debugText.Text = string.Format("Primitive counts - {0} ellipses, {1} lines, {2} points, {3} text, {4} total.",
                        ellipseCount, lineCount, pointCount, textCount, ellipseCount + lineCount + pointCount + textCount);
                }));
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
            // prepare ribbon
            foreach (var ribbonId in Workspace.SettingsManager.RibbonOrder)
            {
                var rib = RibbonTabs.FirstOrDefault(t => t.Metadata.Id == ribbonId);
                if (rib != null)
                    this.ribbon.Items.Add(rib.Value);
            }

            // prepare user console
            var console = Consoles.First(c => c.Metadata.ControlId == Workspace.SettingsManager.ConsoleControlId).Value;
            this.inputPanel.Content = console;
            TakeFocus();
            InputService.Reset();

            Workspace.Update(viewControl: viewPane, isDirty: false);

            var args = Environment.GetCommandLineArgs().Skip(1); // trim off executable
            args = args.Where(a => !a.StartsWith("/")); // remove options
            if (args.Count() == 1)
            {
                var fileName = args.First();
                if (File.Exists(fileName))
                {
                    Workspace.Update(isDirty: false);
                    Workspace.ExecuteCommand("File.Open", fileName);
                }
                else
                {
                    OutputService.WriteLine("Unable to open file: ", fileName);
                }
            }
            else
            {
                SetTitle(Workspace.Drawing);
            }
        }

        private void SetTitle(Drawing drawing)
        {
            string filename = drawing.Settings.FileName == null ? "(Untitled)" : Path.GetFileName(drawing.Settings.FileName);
            Dispatcher.BeginInvoke((Action)(() =>
                this.ribbon.Title = string.Format("BCad [{0}]{1}", filename, Workspace.IsDirty ? " *" : "")));
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (await Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            Workspace.SaveSettings();
        }

        private void StatusBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null)
                return;

            var property = textBlock.Tag as string;
            if (property == null)
                return;

            var propInfo = typeof(ISettingsManager).GetProperty(property);
            if (propInfo == null)
                return;

            bool value = (bool)propInfo.GetValue(Workspace.SettingsManager, null);
            propInfo.SetValue(Workspace.SettingsManager, !value, null);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Workspace == null ? false : Workspace.CanExecute();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(Workspace != null, "Workspace should not have been null");
            var command = e.Parameter as string;
            Debug.Assert(command != null, "Command string should not have been null");
            Workspace.ExecuteCommand(command);
        }
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;

            return val ? Brushes.Black : Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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
                if (foo != null)
                {
                    UIElement elem = (UIElement)foo;
                    elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (MethodInvoker)delegate()
                        {
                            elem.Focus();
                            Keyboard.Focus(elem);
                        });
                }
            }, element);
        }
    }
}
