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
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using BCad.Commands;
using BCad.EventArguments;
using BCad.Primitives;
using BCad.Ribbons;
using BCad.ViewModels;

namespace BCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private EditPaneViewModel editViewModel;

        public MainWindow()
        {
            InitializeComponent();

            App.Container.SatisfyImports(this);
        }

        [Import]
        public IWorkspace Workspace { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<RibbonTab, RibbonTabMetadata>> RibbonTabs { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ICadCommand, CadCommandMetadata>> Commands { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            editViewModel = new EditPaneViewModel(Workspace);
            editPane.DataContext = editViewModel;

            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
            Workspace.SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;

            // prepare status bar bindings
            foreach (var x in new[] { new { TextBlock = this.orthoStatus, Path = nameof(Workspace.SettingsManager.Ortho) },
                                      new { TextBlock = this.pointSnapStatus, Path = nameof(Workspace.SettingsManager.PointSnap) },
                                      new { TextBlock = this.angleSnapStatus, Path = nameof(Workspace.SettingsManager.AngleSnap) },
                                      new { TextBlock = this.debugStatus, Path = nameof(Workspace.SettingsManager.Debug) }})
            {
                var binding = new Binding(x.Path);
                binding.Source = Workspace.SettingsManager;
                binding.Converter = new BoolToBrushConverter();
                x.TextBlock.SetBinding(TextBlock.ForegroundProperty, binding);
            }

            // add keyboard shortcuts for command bindings
            foreach (var command in from c in Commands
                                    let metadata = c.Metadata
                                    where metadata.Key != BCad.Commands.Key.None
                                       || metadata.Modifier != BCad.Commands.ModifierKeys.None
                                    select metadata)
            {
                this.InputBindings.Add(new InputBinding(
                    new UserCommand(this.Workspace, command.Name),
                    new KeyGesture((System.Windows.Input.Key)command.Key, (System.Windows.Input.ModifierKeys)command.Modifier)));
            }

            // add keyboard shortcuts for toggled settings
            var uiSettings = Workspace.SettingsManager as SettingsManager;
            Debug.Assert(uiSettings != null);
            foreach (var setting in new[] {
                new { Name = nameof(Workspace.SettingsManager.AngleSnap), Shortcut = uiSettings.AngleSnapShortcut },
                new { Name = nameof(Workspace.SettingsManager.PointSnap), Shortcut = uiSettings.PointSnapShortcut },
                new { Name = nameof(Workspace.SettingsManager.Ortho), Shortcut = uiSettings.OrthoShortcut },
                new { Name = nameof(Workspace.SettingsManager.Debug), Shortcut = uiSettings.DebugShortcut } })
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
                case nameof(Workspace.SettingsManager.Debug):
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
            this.Dispatcher.BeginInvoke((Action)(() => FocusHelper.Focus(this.inputPanel)));
        }

        void Workspace_CommandExecuted(object sender, CadCommandExecutedEventArgs e)
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
            App.Container.SatisfyImports(inputPanel);
            TakeFocus();
            Workspace.InputService.Reset();

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
                    Workspace.OutputService.WriteLine("Unable to open file: ", fileName);
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
