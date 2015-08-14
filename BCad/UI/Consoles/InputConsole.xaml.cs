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

        [Import]
        public IWorkspace Workspace { get; set; }

        private IInputService _inputService;
        private IOutputService _outputService;

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            _inputService = Workspace.GetService<IInputService>();
            _outputService = Workspace.GetService<IOutputService>();

            _inputService.PromptChanged += HandlePromptChanged;
            _outputService.LineWritten += HandleLineWritten;
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
                    if (_inputService.AllowedInputTypes.HasFlag(InputType.Command))
                    {
                        e.Handled = true;
                    }

                    if (!_inputService.AllowedInputTypes.HasFlag(InputType.Text))
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
            _inputService.Cancel();

            inputLine.Text = string.Empty;
        }

        private void SubmitValue()
        {
            var text = inputLine.Text;

            if (_inputService.AllowedInputTypes.HasFlag(InputType.Directive) &&
                _inputService.AllowedDirectives.Contains(text))
            {
                _inputService.PushDirective(text);
            }
            else if (_inputService.AllowedInputTypes.HasFlag(InputType.Distance))
            {
                double dist = 0.0;
                if (string.IsNullOrEmpty(text))
                {
                    _inputService.PushNone();
                }
                else if (DrawingSettings.TryParseUnits(text, out dist))
                {
                    _inputService.PushDistance(dist);
                }
            }
            else if (_inputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                Point point;
                var cursorPoint = Workspace.ViewControl.GetCursorPoint().Result;
                if (_inputService.TryParsePoint(text, cursorPoint, _inputService.LastPoint, out point))
                    _inputService.PushPoint(point);
            }
            else if (_inputService.AllowedInputTypes.HasFlag(InputType.Command))
            {
                _inputService.PushCommand(string.IsNullOrEmpty(text) ? null : text);
            }
            else if (_inputService.AllowedInputTypes.HasFlag(InputType.Text))
            {
                _inputService.PushText(text ?? string.Empty);
            }

            inputLine.Text = string.Empty;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputLine.Focus();
        }
    }
}
