using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BCad.EventArguments;
using BCad.Helpers;

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
        }

        private void HandlePromptChanged(object sender, PromptChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => prompt.Content = e.Prompt));
        }

        private void HandleLineWritten(object sender, WriteLineEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                history.AppendText(e.Line + Environment.NewLine);
                history.ScrollToEnd();
            }));
        }

        public UserControl Control { get { return this; } }

        [Import]
        private IInputService InputService = null;

        [Import]
        private IView View = null;

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.LineFeed:
                    SubmitValue();
                    break;
                case Key.Space:
                    if (InputService.DesiredInputType != InputType.Text)
                        e.Handled = true;
                    SubmitValue();
                    break;
                case Key.Escape:
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
            if (InputService.DesiredInputType == InputType.Entity)
                return; // doesn't make sense
            object value = null;
            switch (InputService.DesiredInputType)
            {
                case InputType.Point:
                    var point = ParsePoint(inputLine.Text);
                    if (point != null)
                        value = point;
                    else
                        value = inputLine.Text; // directive
                    break;
                case InputType.Command:
                case InputType.Text:
                    value = string.IsNullOrEmpty(inputLine.Text) ? null : inputLine.Text;
                    break;
            }
            InputService.PushValue(value);

            inputLine.Text = string.Empty;
        }

        private static Regex numberPattern = new Regex("^" + Point.NumberPattern + "$", RegexOptions.Compiled);

        private static Regex relativePoint = new Regex(string.Format("^@{0},{0},{0}$", Point.NumberPattern), RegexOptions.Compiled);

        private static Regex relativeAngle = new Regex(string.Format("^{0}<{0}$", Point.NumberPattern), RegexOptions.Compiled);

        private Point ParsePoint(string text)
        {
            // if only 2 coordinates given
            if (text.Count(c => c == ',') == 1)
                text += ",0";

            Point p;
            if (numberPattern.IsMatch(text))
            {
                // length on current vector
                var length = double.Parse(text);
                var cursor = View.RegisteredControl.GetCursorPoint();
                var vec = cursor - InputService.LastPoint;
                if (vec.LengthSquared == 0.0)
                {
                    // if no change report the last point
                    p = InputService.LastPoint;
                }
                else
                {
                    vec = vec.Normalize() * length;
                    p = (InputService.LastPoint + vec).ToPoint();
                }
            }
            else if (relativePoint.IsMatch(text))
            {
                // offset from last point
                var offset = Point.Parse(text.Substring(1));
                p = (InputService.LastPoint + offset).ToPoint();
            }
            else if (relativeAngle.IsMatch(text))
            {
                // distance and angle
                var parts = text.Split("<".ToCharArray(), 2);
                var dist = double.Parse(parts[0]);
                var angle = double.Parse(parts[1]);
                var radians = angle * MathHelper.DegreesToRadians;
                var offset = new Vector(Math.Cos(radians), Math.Sin(radians), 0) * dist;
                p = (InputService.LastPoint + offset).ToPoint();
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
