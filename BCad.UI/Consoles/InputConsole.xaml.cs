using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using BCad.EventArguments;
using BCad.Helpers;
using BCad.Services;
using Input = System.Windows.Input;

namespace BCad.UI.Consoles
{
    /// <summary>
    /// Interaction logic for InputConsole.xaml
    /// </summary>
    [ExportConsole("Default")]
    public partial class InputConsole : ConsoleControl, IPartImportsSatisfiedNotification
    {
        public InputConsole()
        {
            InitializeComponent();
            //inputLine.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(InputKeyDown), true);
        }

        public void OnImportsSatisfied()
        {
            InputService.PromptChanged += HandlePromptChanged;
            InputService.LineWritten += HandleLineWritten;
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

        private void WorkspaceCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                inputLine.Text = "";
            }));
        }

        public UserControl Control { get { return this; } }

        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

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

            if (InputService.AllowedInputTypes.HasFlag(InputType.Directive))
            {
                if (InputService.AllowedDirectives.Contains(text))
                {
                    InputService.PushDirective(text);
                }
                else
                {
                    InputService.PushNone();
                }
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
                var point = ParsePoint(text);
                if (point != null)
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

        private static Regex numberPattern = new Regex("^" + Point.NumberPattern + "$", RegexOptions.Compiled);

        private static Regex relativePoint = new Regex(string.Format("^@{0},{0},{0}$", Point.NumberPattern), RegexOptions.Compiled);

        private static Regex relativeAngle = new Regex(string.Format("^.*<{0}$", Point.NumberPattern), RegexOptions.Compiled);

        private Point ParsePoint(string text)
        {
            // if only 2 coordinates given
            if (text.Count(c => c == ',') == 1)
                text += ",0";

            Point p;
            double value = 0.0;
            if (DrawingSettings.TryParseUnits(text, out value))
            {
                // length on current vector
                var length = value;
                var cursor = Workspace.ViewControl.GetCursorPoint();
                var vec = cursor - InputService.LastPoint;
                if (vec.LengthSquared == 0.0)
                {
                    // if no change report the last point
                    p = InputService.LastPoint;
                }
                else
                {
                    vec = vec.Normalize() * length;
                    p = InputService.LastPoint + vec;
                }
            }
            else if (relativePoint.IsMatch(text))
            {
                // offset from last point
                var offset = Point.Parse(text.Substring(1));
                p = InputService.LastPoint + offset;
            }
            else if (relativeAngle.IsMatch(text))
            {
                // distance and angle
                var parts = text.Split("<".ToCharArray(), 2);
                if (DrawingSettings.TryParseUnits(parts[0], out value))
                {
                    var dist = value;
                    var angle = double.Parse(parts[1]);
                    var radians = angle * MathHelper.DegreesToRadians;
                    var offset = new Vector(Math.Cos(radians), Math.Sin(radians), 0) * dist;
                    p = InputService.LastPoint + offset;
                }
                else
                {
                    // failed to parse distance
                    p = null;
                }
            }
            else if (Point.PointPattern.IsMatch(text))
            {
                // absolute point
                p = Point.Parse(text);
            }
            else
            {
                // invalid point
                p = null;
            }

            return p;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputLine.Focus();
        }
    }
}
