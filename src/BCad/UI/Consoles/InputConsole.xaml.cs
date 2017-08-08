// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Services;
using Input = System.Windows.Input;

namespace IxMilia.BCad.UI.Consoles
{
    /// <summary>
    /// Interaction logic for InputConsole.xaml
    /// </summary>
    public partial class InputConsole : UserControl
    {
        public InputConsole()
        {
            InitializeComponent();
            //inputLine.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(InputKeyDown), true);
        }

        [Import]
        public IWorkspace Workspace { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            Workspace.InputService.PromptChanged += HandlePromptChanged;
            Workspace.OutputService.LineWritten += HandleLineWritten;
            Workspace.CommandExecuted += WorkspaceCommandExecuted;
        }

        private void HandlePromptChanged(object sender, PromptChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                inputLine.Text = "";
                prompt.Content = e.Prompt;
            }));
        }

        private void HandleLineWritten(object sender, WriteLineEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                history.AppendText(e.Line + Environment.NewLine);
                history.ScrollToEnd();
            }));
        }

        private void WorkspaceCommandExecuted(object sender, CadCommandExecutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                inputLine.Text = "";
            }));
        }

        private void InputKeyDown(object sender, Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Input.Key.Enter:
                case Input.Key.LineFeed:
                    SubmitValue();
                    break;
                case Input.Key.Space:
                    if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Command))
                    {
                        e.Handled = true;
                    }

                    if (!Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Text))
                    {
                        // space doesn't submit when getting text
                        SubmitValue();
                    }
                    break;
                case Input.Key.Escape:
                    SubmitCancel();
                    break;
                default:
                    break;
            }
        }

        private void SubmitCancel()
        {
            Workspace.InputService.Cancel();
            inputLine.Text = string.Empty;
        }

        private void SubmitValue()
        {
            var text = inputLine.Text;

            if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Directive) &&
                Workspace.InputService.AllowedDirectives.Contains(text))
            {
                Workspace.InputService.PushDirective(text);
            }
            else if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Distance))
            {
                double dist = 0.0;
                if (string.IsNullOrEmpty(text))
                {
                    Workspace.InputService.PushNone();
                }
                else if (DrawingSettings.TryParseUnits(text, out dist))
                {
                    Workspace.InputService.PushDistance(dist);
                }
            }
            else if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                Point point;
                var cursorPoint = Workspace.ViewControl.GetCursorPoint(CancellationToken.None).Result;
                if (Workspace.InputService.TryParsePoint(text, cursorPoint, Workspace.InputService.LastPoint, out point))
                    Workspace.InputService.PushPoint(point);
            }
            else if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Command))
            {
                Workspace.InputService.PushCommand(string.IsNullOrEmpty(text) ? null : text);
            }
            else if (Workspace.InputService.AllowedInputTypes.HasFlag(InputType.Text))
            {
                Workspace.InputService.PushText(text ?? string.Empty);
            }

            inputLine.Text = string.Empty;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputLine.Focus();
        }
    }
}
