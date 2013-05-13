using System.Collections.Generic;
using System.Threading.Tasks;
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
        Text = 0x40
    }

    public delegate void PromptChangedEventHandler(object sender, PromptChangedEventArgs e);
    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs e);
    public delegate void ValueRequestedEventHandler(object sender, ValueRequestedEventArgs e);
    public delegate void ValueReceivedEventHandler(object sender, ValueReceivedEventArgs e);
    public delegate void RubberBandGeneratorChangedEventHandler(object sender, RubberBandGeneratorChangedEventArgs e);
    public delegate IEnumerable<IPrimitive> RubberBandGenerator(Point point);

    public interface IInputService
    {
        Point LastPoint { get; }
        RubberBandGenerator PrimitiveGenerator { get; }
        Task<ValueOrDirective<double>> GetDistance(string prompt = null, double? defaultDistance = null);
        Task<ValueOrDirective<Point>> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null, Point lastPoint = null);
        Task<ValueOrDirective<SelectedEntity>> GetEntity(UserDirective directive, RubberBandGenerator onCursorMove = null);
        Task<ValueOrDirective<IEnumerable<Entity>>> GetEntities(string prompt = null, RubberBandGenerator onCursorMove = null);
        Task<ValueOrDirective<string>> GetText(string prompt = null);

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
        void PushText(string text);

        InputType AllowedInputTypes { get; }
        IEnumerable<string> AllowedDirectives { get; }
        bool IsDrawing { get; }

        event PromptChangedEventHandler PromptChanged;
        event WriteLineEventHandler LineWritten;
        event ValueRequestedEventHandler ValueRequested;
        event ValueReceivedEventHandler ValueReceived;
        event RubberBandGeneratorChangedEventHandler RubberBandGeneratorChanged;

        void Reset();

        bool TryParsePoint(string text, Point currentCursor, Point lastPoint, out Point point);
    }
}
