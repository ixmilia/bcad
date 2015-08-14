using System;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BCad.EventArguments;
using BCad.Services;
using Input = System.Windows.Input;

namespace BCad.UI.Consoles
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

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            InputService.PromptChanged += HandlePromptChanged;
            OutputService.LineWritten += HandleLineWritten;
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

        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        private void InputKeyDown(object sender, Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Input.Key.Enter:
                case Input.Key.LineFeed:
                    SubmitValue();
                    break;
                case Input.Key.Space:
                    if (InputService.AllowedInputTypes.HasFlag(InputType.Command))
                    {
                        e.Handled = true;
                    }

                    if (!InputService.AllowedInputTypes.HasFlag(InputType.Text))
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
            InputService.Cancel();

            inputLine.Text = string.Empty;
        }

        private void SubmitValue()
        {
            var text = inputLine.Text;

            if (InputService.AllowedInputTypes.HasFlag(InputType.Directive) &&
                InputService.AllowedDirectives.Contains(text))
            {
                InputService.PushDirective(text);
            }
            else if (InputService.AllowedInputTypes.HasFlag(InputType.Distance))
            {
                double dist = 0.0;
                if (string.IsNullOrEmpty(text))
                {
                    InputService.PushNone();
                }
                else if (DrawingSettings.TryParseUnits(text, out dist))
                {
                    InputService.PushDistance(dist);
                }
            }
            else if (InputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                Point point;
                var cursorPoint = Workspace.ViewControl.GetCursorPoint().Result;
                if (InputService.TryParsePoint(text, cursorPoint, InputService.LastPoint, out point))
                    InputService.PushPoint(point);
            }
            else if (InputService.AllowedInputTypes.HasFlag(InputType.Command))
            {
                InputService.PushCommand(string.IsNullOrEmpty(text) ? null : text);
            }
            else if (InputService.AllowedInputTypes.HasFlag(InputType.Text))
            {
                InputService.PushText(text ?? string.Empty);
            }

            inputLine.Text = string.Empty;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputLine.Focus();
        }
    }
}
