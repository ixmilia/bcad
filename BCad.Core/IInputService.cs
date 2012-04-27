using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using BCad.EventArguments;
using BCad.Objects;

namespace BCad
{
    public enum InputType
    {
        None,
        Command,
        Directive,
        Point,
        Object,
        Text
    }

    public delegate void PromptChangedEventHandler(object sender, PromptChangedEventArgs e);

    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs e);

    public delegate void ValueRequestedEventHandler(object sender, ValueRequestedEventArgs e);

    public delegate void ValueReceivedEventHandler(object sender, ValueReceivedEventArgs e);

    public delegate void RubberBandGeneratorChangedEventHandler(object sender, RubberBandGeneratorChangedEventArgs e);

    public delegate IEnumerable<IPrimitive> RubberBandGenerator(Point point);

    public interface IInputService
    {
        RubberBandGenerator PrimitiveGenerator { get; }

        ValueOrDirective<Point> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null);

        ValueOrDirective<Entity> GetObject(UserDirective directive, RubberBandGenerator onCursorMove = null);

        Point LastPoint { get; }

        void WriteLine(string text);
        void WriteLine(string text, params object[] param);

        void Cancel();
        void PushValue(object value);

        InputType DesiredInputType { get; }
        bool IsDrawing { get; }

        event PromptChangedEventHandler PromptChanged;

        event WriteLineEventHandler LineWritten;

        event ValueRequestedEventHandler ValueRequested;

        event ValueReceivedEventHandler ValueReceived;

        event RubberBandGeneratorChangedEventHandler RubberBandGeneratorChanged;

        void Reset();
    }
}
