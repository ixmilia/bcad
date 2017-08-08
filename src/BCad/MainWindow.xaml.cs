// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
using IxMilia.BCad.Commands;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Ribbons;
using IxMilia.BCad.Settings;
using IxMilia.BCad.ViewModels;

namespace IxMilia.BCad
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

            CompositionContainer.Container.SatisfyImports(this);
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
            Workspace.SettingsService.SettingChanged += SettingsManager_PropertyChanged;

            // prepare status bar bindings
            var vm = new StatusBarViewModel(Workspace.SettingsService);
            foreach (var x in new[] { new { TextBlock = this.orthoStatus, Path = nameof(WpfSettingsProvider.Ortho) },
                                      new { TextBlock = this.pointSnapStatus, Path = nameof(WpfSettingsProvider.PointSnap) },
                                      new { TextBlock = this.angleSnapStatus, Path = nameof(WpfSettingsProvider.AngleSnap) },
                                      new { TextBlock = this.debugStatus, Path = nameof(DefaultSettingsProvider.Debug) }})
            {
                var binding = new Binding(x.Path);
                binding.Source = vm;
                binding.Converter = new BoolToBrushConverter();
                binding.Mode = BindingMode.TwoWay;
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
            foreach (var setting in new[] {
                new { Name = nameof(WpfSettingsProvider.AngleSnap), Shortcut = Workspace.SettingsService.GetValue<KeyboardShortcut>(WpfSettingsProvider.AngleSnapShortcut) },
                new { Name = nameof(WpfSettingsProvider.PointSnap), Shortcut = Workspace.SettingsService.GetValue<KeyboardShortcut>(WpfSettingsProvider.PointSnapShortcut) },
                new { Name = nameof(WpfSettingsProvider.Ortho), Shortcut = Workspace.SettingsService.GetValue<KeyboardShortcut>(WpfSettingsProvider.OrthoShortcut) },
                new { Name = nameof(DefaultSettingsProvider.Debug), Shortcut = Workspace.SettingsService.GetValue<KeyboardShortcut>(WpfSettingsProvider.DebugShortcut) } })
            {
                if (setting.Shortcut.HasValue)
                {
                    this.InputBindings.Add(new InputBinding(
                        new ToggleSettingsCommand(Workspace.SettingsService, setting.Name),
                        new KeyGesture(setting.Shortcut.Key, setting.Shortcut.Modifier)));
                }
            }

            Workspace_WorkspaceChanged(this, WorkspaceChangeEventArgs.Reset());
        }

        private void SettingsManager_PropertyChanged(object sender, SettingChangedEventArgs e)
        {
            switch (e.SettingName)
            {
                case nameof(DefaultSettingsProvider.Debug):
                    if (Workspace.SettingsService.GetValue<bool>(DefaultSettingsProvider.Debug))
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
                if (Workspace.SettingsService.GetValue<bool>(DefaultSettingsProvider.Debug))
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
            foreach (var ribbonId in Workspace.SettingsService.GetValue<string[]>(WpfSettingsProvider.RibbonOrder))
            {
                var rib = RibbonTabs.FirstOrDefault(t => t.Metadata.Id == ribbonId);
                if (rib != null)
                    this.ribbon.Items.Add(rib.Value);
            }

            // prepare user console
            CompositionContainer.Container.SatisfyImports(inputPanel);
            TakeFocus();
            Workspace.InputService.Reset();

            Workspace.Update(isDirty: false);

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

            ((WpfWorkspace)Workspace).SaveSettings();
        }

        private void StatusBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null)
                return;

            var settingName = textBlock.Tag as string;
            if (settingName == null)
                return;

            var currentValue = Workspace.SettingsService.GetValue<bool>(settingName);
            Workspace.SettingsService.SetValue(settingName, !currentValue);
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
