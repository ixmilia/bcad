using System.Collections.Generic;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Primitives;

namespace BCad.Services
{
    public enum InputType
    {
        None = 0x00,
        Command = 0x01,
        Distance = 0x02,
        Directive = 0x04,
        Point = 0x08,
        Entity = 0x10,
        Entities = 0x20,
        //Text = 0x40
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
        ValueOrDirective<double> GetDistance(double? defaultDistance = null);
        ValueOrDirective<Point> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null);
        ValueOrDirective<SelectedEntity> GetEntity(UserDirective directive, RubberBandGenerator onCursorMove = null);
        ValueOrDirective<IEnumerable<Entity>> GetEntities(string prompt = null, RubberBandGenerator onCursorMove = null);
        Point LastPoint { get; }

        void WriteLine(string text);
        void WriteLine(string text, params object[] param);

        void WriteLineDebug(string text);
        void WriteLineDebug(string text, params object[] param);

        void Cancel();
        void PushNone();
        void PushCommand(string commandName);
        void PushDistance(double distance);
        void PushDirective(string directive);
        void PushPoint(Point point);
        void PushEntity(SelectedEntity entity);
        void PushEntities(IEnumerable<Entity> entities);

        InputType AllowedInputTypes { get; }
        IEnumerable<string> AllowedDirectives { get; }
        bool IsDrawing { get; }

        event PromptChangedEventHandler PromptChanged;
        event WriteLineEventHandler LineWritten;
        event ValueRequestedEventHandler ValueRequested;
        event ValueReceivedEventHandler ValueReceived;
        event RubberBandGeneratorChangedEventHandler RubberBandGeneratorChanged;

        void Reset();
    }
}
