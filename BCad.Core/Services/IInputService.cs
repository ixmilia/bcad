using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.EventArguments;

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
    public delegate void ValueRequestedEventHandler(object sender, ValueRequestedEventArgs e);
    public delegate void ValueReceivedEventHandler(object sender, ValueReceivedEventArgs e);

    public interface IInputService : IWorkspaceService
    {
        Point LastPoint { get; }
        Task<ValueOrDirective<double>> GetDistance(string prompt = null, Optional<double> defaultDistance = default(Optional<double>));
        Task<ValueOrDirective<Point>> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null, Optional<Point> lastPoint = default(Optional<Point>));
        Task<ValueOrDirective<SelectedEntity>> GetEntity(UserDirective directive, RubberBandGenerator onCursorMove = null);
        Task<ValueOrDirective<IEnumerable<Entity>>> GetEntities(string prompt = null, EntityKind entityKinds = EntityKind.All, RubberBandGenerator onCursorMove = null);
        Task<ValueOrDirective<string>> GetText(string prompt = null);

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

        event PromptChangedEventHandler PromptChanged;
        event ValueRequestedEventHandler ValueRequested;
        event ValueReceivedEventHandler ValueReceived;
        event EventHandler InputCanceled;

        void Reset();

        bool TryParsePoint(string text, Point currentCursor, Point lastPoint, out Point point);
    }
}
