using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCad.EventArguments;
using BCad.Objects;

namespace BCad
{
    /// <summary>
    /// Interaction logic for UserConsole.xaml
    /// </summary>
    public partial class WpfConsole : UserControl, IPartImportsSatisfiedNotification
    {
        public WpfConsole()
        {
            InitializeComponent();
            //inputLine.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(InputKeyDown), true);
        }

        public void OnImportsSatisfied()
        {
            UserConsole.PromptChanged += HandlePromptChanged;
            UserConsole.LineWritten += HandleLineWritten;
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
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IView View { get; set; }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.LineFeed:
                    SubmitValue();
                    break;
                case Key.Space:
                    if (UserConsole.DesiredInputType != InputType.Text)
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
            UserConsole.Cancel();

            inputLine.Text = string.Empty;
        }

        private void SubmitValue()
        {
            if (UserConsole.DesiredInputType == InputType.Object)
                return; // doesn't make sense
            object value = null;
            switch (UserConsole.DesiredInputType)
            {
                case InputType.Point:
                    var point = ParsePoint(inputLine.Text);
                    if (point != null)
                        value = point;
                    else
                        value = inputLine.Text; // directive
                    break;
                case InputType.Object:
                    // TODO: submit object
                    break;
                case InputType.Command:
                case InputType.Text:
                    value = string.IsNullOrEmpty(inputLine.Text) ? null : inputLine.Text;
                    break;
            }
            UserConsole.PushValue(value);

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
                var vec = View.GetCursorPoint() - UserConsole.LastPoint;
                if (vec.LengthSquared == 0.0)
                {
                    // if no change report the last point
                    p = UserConsole.LastPoint;
                }
                else
                {
                    vec.Normalize();
                    vec *= length;
                    p = (UserConsole.LastPoint + vec).ToPoint();
                }
            }
            else if (relativePoint.IsMatch(text))
            {
                // offset from last point
                var offset = Point.Parse(text.Substring(1));
                p = (UserConsole.LastPoint + offset).ToPoint();
            }
            else if (relativeAngle.IsMatch(text))
            {
                // distance and angle
                var parts = text.Split("<".ToCharArray(), 2);
                var dist = double.Parse(parts[0]);
                var angle = double.Parse(parts[1]);
                var radians = angle * Math.PI / 180.0;
                var offset = new Vector(Math.Cos(radians), Math.Sin(radians), 0) * dist;
                p = (UserConsole.LastPoint + offset).ToPoint();
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
