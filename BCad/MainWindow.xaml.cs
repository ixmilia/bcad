using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using BCad.Commands;
using BCad.EventArguments;
using BCad.UI;
using Microsoft.Windows.Controls.Ribbon;
using BCad.Entities;

namespace BCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, IPartImportsSatisfiedNotification
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
        private IEnumerable<RibbonTab> RibbonTabs = null; // TODO: import lazily and sort by name

        [ImportMany]
        private IEnumerable<Lazy<ViewControl, IViewControlMetadata>> Views = null;

        [ImportMany]
        private IEnumerable<Lazy<ConsoleControl, IConsoleMetadata>> Consoles = null;

        [ImportMany]
        private IEnumerable<Lazy<BCad.Commands.ICommand, ICommandMetadata>> Commands = null;

        public void OnImportsSatisfied()
        {
            Workspace.CommandExecuted += Workspace_CommandExecuted;
            Workspace.PropertyChanged += Workspace_PropertyChanged;

            // prepare status bar bindings
            foreach (var x in new[] { new { TextBlock = this.orthoStatus, Path = Constants.OrthoString },
                                      new { TextBlock = this.pointSnapStatus, Path = Constants.PointSnapString },
                                      new { TextBlock = this.angleSnapStatus, Path = Constants.AngleSnapString }})
            {
                var binding = new Binding(x.Path);
                binding.Source = Workspace.SettingsManager;
                binding.Converter = new BoolToBrushConverter();
                x.TextBlock.SetBinding(TextBlock.ForegroundProperty, binding);
            }

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
            foreach (var setting in new[] {
                                        new { Name = Constants.AngleSnapString, Shortcut = Workspace.SettingsManager.AngleSnapShortcut },
                                        new { Name = Constants.PointSnapString, Shortcut = Workspace.SettingsManager.PointSnapShortcut },
                                        new { Name = Constants.PointSnapString, Shortcut = Workspace.SettingsManager.OrthoShortcut } })
            {
                if (setting.Shortcut.HasValue)
                {
                    this.InputBindings.Add(new InputBinding(
                        new ToggleSettingsCommand(Workspace.SettingsManager, setting.Name),
                        new KeyGesture(setting.Shortcut.Key, setting.Shortcut.Modifier)));
                }
            }

            Workspace_PropertyChanged(this, new PropertyChangedEventArgs(Constants.DrawingString));
        }

        void Workspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Constants.CurrentLayerString:
                    TakeFocus();
                    break;
                case Constants.DrawingString:
                    TakeFocus();
                    SetTitle(Workspace.Drawing);
                    int lineCount = 0, arcCount = 0, circleCount = 0, ellipseCount = 0, textCount = 0;
                    foreach (var ent in Workspace.Drawing.Layers.SelectMany(l => l.Value.Entities))
                    {
                        switch (ent.Kind)
                        {
                            case EntityKind.Arc:
                                arcCount++;
                                break;
                            case EntityKind.Circle:
                                circleCount++;
                                break;
                            case EntityKind.Ellipse:
                                ellipseCount++;
                                break;
                            case EntityKind.Line:
                                lineCount++;
                                break;
                            case EntityKind.Text:
                                textCount++;
                                break;
                        }
                    }
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    debugStatus.Text = string.Format("Entity counts - {0} arcs, {1} circles, {2} ellipses, {3} lines, {4} text, {5} total.",
                        arcCount, circleCount, ellipseCount, lineCount, textCount, arcCount + circleCount + ellipseCount + lineCount + textCount)));
                    break;
                default:
                    break;
            }
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
            // TODO: order as specified in settings
            foreach (var tab in RibbonTabs)
            {
                this.ribbon.Items.Add(tab);
            }

            // prepare user console
            var console = Consoles.First(c => c.Metadata.ControlId == Workspace.SettingsManager.ConsoleControlId).Value;
            this.inputPanel.Content = console;
            TakeFocus();
            InputService.Reset();

            // prepare view control
            var view = Views.First(v => v.Metadata.ControlId == Workspace.SettingsManager.ViewControlId).Value;
            this.viewPanel.Content = view;
            View.RegisteredControl = view;

            View.UpdateView(viewPoint: new Point(0, 0, 1),
                sight: Vector.ZAxis,
                up: Vector.YAxis,
                viewWidth: 300.0,
                bottomLeft: new Point(-10, -10, 0));

            SetTitle(Workspace.Drawing);
        }

        private void SetTitle(Drawing drawing)
        {
            string filename = drawing.FileName == null ? "(Untitled)" : Path.GetFileName(drawing.FileName);
            Dispatcher.BeginInvoke((Action)(() =>
                this.ribbon.Title = string.Format("BCad [{0}]{1}", filename, drawing.IsDirty ? " *" : "")));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
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
