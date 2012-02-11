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
        Command,
        Directive,
        Point,
        Object,
        Text
    }

    public delegate void PromptChangedEventHandler(object sender, PromptChangedEventArgs e);

    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs e);

    public delegate void ValueReceivedEventHandler(object sender, ValueReceivedEventArgs e);

    public delegate void RubberBandGeneratorChangedEventHandler(object sender, RubberBandGeneratorChangedEventArgs e);

    public delegate void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

    public delegate IEnumerable<IPrimitive> RubberBandGenerator(Point point);

    public interface IUserConsole
    {
        RubberBandGenerator PrimitiveGenerator { get; }

        ValueOrDirective<Point> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null);

        ValueOrDirective<IObject> GetObject(UserDirective directive, RubberBandGenerator onCursorMove = null);

        Point LastPoint { get; }

        void WriteLine(string text);
        void WriteLine(string text, params object[] param);

        void Cancel();
        void PushValue(object value);

        InputType DesiredInputType { get; }

        event PromptChangedEventHandler PromptChanged;

        event WriteLineEventHandler LineWritten;

        event ValueReceivedEventHandler ValueReceived;

        event RubberBandGeneratorChangedEventHandler RubberBandGeneratorChanged;

        event CommandExecutingEventHandler CommandExecuting;

        event CommandExecutedEventHandler CommandExecuted;

        void Reset();
    }
}
