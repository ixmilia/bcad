using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Services
{
    internal class InputServiceLogEntry : LogEntry
    {
        public InputType InputType { get; private set; }

        public object Value { get; private set; }

        public InputServiceLogEntry(InputType inputType, object value)
        {
            InputType = inputType;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("input: {0} {1}", InputType, Value);
        }
    }

    internal class InputService : IInputService
    {
        private IWorkspace _workspace;

        public InputService(IWorkspace workspace)
        {
            _workspace = workspace;
            LastPoint = Point.Origin;
            Reset();
            _workspace.CommandExecuted += Workspace_CommandExecuted;
        }

        void Workspace_CommandExecuted(object sender, CadCommandExecutedEventArgs e)
        {
            _workspace.SelectedEntities.Clear();
            SetPrompt("Command");
        }

        private object callbackGate = new object();

        public Point LastPoint { get; private set; }

        private UserDirective currentDirective = null;

        public async Task<ValueOrDirective<double>> GetDistance(UserDirective directive = null, Func<double, IEnumerable<IPrimitive>> onCursorMove = null, Optional<double> defaultDistance = default(Optional<double>))
        {
            var allowedType = InputType.Distance | InputType.Directive | InputType.Point;
            OnValueRequested(new ValueRequestedEventArgs(allowedType));
            if (directive == null)
            {
                var prompt = "Offset distance or first point";
                if (defaultDistance.HasValue)
                {
                    prompt += $" [{defaultDistance.Value}]";
                }

                directive = new UserDirective(prompt);
            }

            await WaitFor(allowedType, directive, null);
            ValueOrDirective<double> result;
            switch (lastType)
            {
                case PushedValueType.Cancel:
                    result = ValueOrDirective<double>.GetCancel();
                    break;
                case PushedValueType.None:
                    result = ValueOrDirective<double>.GetValue(defaultDistance.Value);
                    break;
                case PushedValueType.Distance:
                    result = ValueOrDirective<double>.GetValue(pushedDistance);
                    break;
                case PushedValueType.Directive:
                    result = ValueOrDirective<double>.GetDirective(pushedDirective);
                    break;
                case PushedValueType.Point:
                    var first = pushedPoint;
                    ResetWaiters();
                    onCursorMove ??= _ => Enumerable.Empty<IPrimitive>();
                    var second = await GetPoint(new UserDirective("Second point of offset distance"), p =>
                    {
                        var distance = (p - first).Length;
                        return new[] { new PrimitiveLine(first, p) }.Concat(onCursorMove(distance));
                    });
                    if (second.HasValue)
                    {
                        var dist = (second.Value - first).Length;
                        result = ValueOrDirective<double>.GetValue(dist);
                    }
                    else if (second.Directive != null)
                    {
                        result = ValueOrDirective<double>.GetDirective(second.Directive);
                    }
                    else
                    {
                        result = ValueOrDirective<double>.GetCancel();
                    }
                    break;
                default:
                    throw new Exception("Unexpected pushed value");
            }

            ResetWaiters();
            return result;
        }

        public async Task<ValueOrDirective<Point>> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null, Optional<Point> lastPoint = default(Optional<Point>))
        {
            this.LastPoint = lastPoint.HasValue ? lastPoint.Value : this.LastPoint;
            OnValueRequested(new ValueRequestedEventArgs(InputType.Point | InputType.Directive));
            await WaitFor(InputType.Point | InputType.Directive, directive, onCursorMove);
            ValueOrDirective<Point> result;
            switch (lastType)
            {
                case PushedValueType.Cancel:
                    result = ValueOrDirective<Point>.GetCancel();
                    break;
                case PushedValueType.None:
                    result = new ValueOrDirective<Point>(); // TODO: default directive?
                    break;
                case PushedValueType.Directive:
                    result = ValueOrDirective<Point>.GetDirective(pushedDirective);
                    break;
                case PushedValueType.Point:
                    result = ValueOrDirective<Point>.GetValue(pushedPoint);
                    break;
                default:
                    throw new Exception("Unexpected pushed value");
            }

            ResetWaiters();
            return result;
        }

        public async Task<ValueOrDirective<SelectedEntity>> GetEntity(UserDirective directive, RubberBandGenerator onCursorMove = null)
        {
            OnValueRequested(new ValueRequestedEventArgs(InputType.Entity | InputType.Directive));
            await WaitFor(InputType.Entity | InputType.Directive, directive, onCursorMove);
            ValueOrDirective<SelectedEntity> result;
            switch (lastType)
            {
                case PushedValueType.None:
                    result = new ValueOrDirective<SelectedEntity>();
                    break;
                case PushedValueType.Cancel:
                    result = ValueOrDirective<SelectedEntity>.GetCancel();
                    break;
                case PushedValueType.Entity:
                    result = ValueOrDirective<SelectedEntity>.GetValue(pushedEntity);
                    break;
                case PushedValueType.Directive:
                    result = ValueOrDirective<SelectedEntity>.GetDirective(pushedDirective);
                    break;
                default:
                    throw new Exception("Unexpected pushed value");
            }

            ResetWaiters();
            return result;
        }

        public async Task<ValueOrDirective<IEnumerable<Entity>>> GetEntities(string prompt = null, EntityKind entityKinds = EntityKind.All, RubberBandGenerator onCursorMove = null)
        {
            OnValueRequested(new ValueRequestedEventArgs(InputType.Entities | InputType.Directive));
            ValueOrDirective<IEnumerable<Entity>>? result = null;
            HashSet<Entity> entities = new HashSet<Entity>();
            bool awaitingMore = true;
            while (awaitingMore)
            {
                await WaitFor(InputType.Entities | InputType.Directive, new UserDirective(prompt ?? "Select entities", "all"), onCursorMove);
                switch (lastType)
                {
                    case PushedValueType.Cancel:
                        result = ValueOrDirective<IEnumerable<Entity>>.GetCancel();
                        awaitingMore = false;
                        break;
                    case PushedValueType.Directive:
                        Debug.Assert(false, "TODO: allow 'all' directive and 'r' to remove objects from selection");
                        break;
                    case PushedValueType.None:
                        result = ValueOrDirective<IEnumerable<Entity>>.GetValue(entities);
                        awaitingMore = false;
                        break;
                    case PushedValueType.Entity:
                        if ((entityKinds & pushedEntity.Entity.Kind) == pushedEntity.Entity.Kind)
                        {
                            entities.Add(pushedEntity.Entity);
                            _workspace.SelectedEntities.Add(pushedEntity.Entity);
                        }

                        _workspace.OutputService.WriteLine($"Selected 1 entity.  Total {entities.Count} selected.");
                        break;
                    case PushedValueType.Entities:
                        foreach (var e in pushedEntities)
                        {
                            if ((entityKinds & e.Kind) == e.Kind)
                            {
                                entities.Add(e);
                                _workspace.SelectedEntities.Add(e);
                            }
                        }

                        _workspace.OutputService.WriteLine($"Selected {pushedEntities.Count()} entities.  Total {entities.Count} selected.");
                        break;
                    default:
                        throw new Exception("Unexpected pushed value");
                }
            }

            ResetWaiters();
            Debug.Assert(result != null, "result should never be null");
            return result.Value;
        }

        public async Task<ValueOrDirective<string>> GetText(string prompt = null)
        {
            OnValueRequested(new ValueRequestedEventArgs(InputType.Text));
            await WaitFor(InputType.Text, new UserDirective(prompt ?? "Enter text"), null);
            ValueOrDirective<string> result;
            switch (lastType)
            {
                case PushedValueType.Cancel:
                    result = ValueOrDirective<string>.GetCancel();
                    break;
                case PushedValueType.None:
                    result = new ValueOrDirective<string>();
                    break;
                case PushedValueType.Text:
                    result = ValueOrDirective<string>.GetValue(pushedString);
                    break;
                default:
                    throw new Exception("Unexpected pushed value");
            }

            ResetWaiters();
            return result;
        }

        public async Task<ValueOrDirective<bool>> GetNone()
        {
            OnValueRequested(new ValueRequestedEventArgs(InputType.None));
            await WaitFor(InputType.None, new UserDirective("Pan"), null);
            ResetWaiters();
            return ValueOrDirective<bool>.GetValue(false);
        }

        private Task WaitFor(InputType type, UserDirective directive, RubberBandGenerator onCursorMove)
        {
            pushValueDone = new TaskCompletionSource<bool>();
            SetPrompt(directive.Prompt);
            currentDirective = directive;
            AllowedInputTypes = type;
            lastType = PushedValueType.None;
            pushedPoint = default(Point);
            pushedEntity = null;
            pushedDirective = null;
            pushedString = null;
            _workspace.RubberBandGenerator = onCursorMove;
            return pushValueDone.Task;
        }

        private void ResetWaiters()
        {
            AllowedInputTypes = InputType.Command;
            lastType = PushedValueType.None;
            pushedPoint = default(Point);
            pushedEntity = null;
            pushedDirective = null;
            pushedString = null;
            currentDirective = null;
            _workspace.RubberBandGenerator = null;
            pushValueDone = null;
        }

        private enum PushedValueType
        {
            None,
            Cancel,
            Point,
            Entity,
            Entities,
            Directive,
            Distance,
            Text
        }

        private object inputGate = new object();
        private PushedValueType lastType = PushedValueType.None;
        private Point pushedPoint = default(Point);
        private double pushedDistance = 0.0;
        private string pushedDirective = null;
        private string pushedString = null;
        private SelectedEntity pushedEntity = null;
        private IEnumerable<Entity> pushedEntities = null;
        private TaskCompletionSource<bool> pushValueDone = null;

        public void Cancel()
        {
            lock (inputGate)
            {
                lastType = PushedValueType.Cancel;
                AllowedInputTypes = InputType.Command;
                var pvd = pushValueDone;
                if (pvd != null)
                {
                    pvd.SetResult(false);
                }

                OnInputCanceled(new EventArgs());
            }
        }

        public void PushNone()
        {
            lock (inputGate)
            {
                if (AllowedInputTypes == InputType.Command)
                {
                    PushCommand(null);
                }
                else
                {
                    lastType = PushedValueType.None;
                    OnValueReceived(new ValueReceivedEventArgs());
                }
            }
        }

        public void PushCommand(string commandName)
        {
            lock (inputGate)
            {
                OnValueReceived(new ValueReceivedEventArgs(commandName, InputType.Command));
                _workspace.ExecuteCommand(commandName);
            }
        }

        public void PushDirective(string directive)
        {
            if (directive == null)
            {
                throw new ArgumentNullException("directive");
            }

            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Directive))
                {
                    if (currentDirective.AllowableDirectives.Contains(directive))
                    {
                        pushedDirective = directive;
                        lastType = PushedValueType.Directive;
                        OnValueReceived(new ValueReceivedEventArgs(directive, InputType.Directive));
                    }
                    else
                    {
                        _workspace.OutputService.WriteLine("Bad value or directive '{0}'", directive);
                    }
                }
            }
        }

        public void PushDistance(double distance)
        {
            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Distance))
                {
                    pushedDistance = distance;
                    lastType = PushedValueType.Distance;
                    OnValueReceived(new ValueReceivedEventArgs(distance));
                }
            }
        }

        public void PushEntity(SelectedEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Entity))
                {
                    pushedEntity = entity;
                    lastType = PushedValueType.Entity;
                    OnValueReceived(new ValueReceivedEventArgs(entity));
                }
            }
        }

        public void PushEntities(IEnumerable<Entity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Entities))
                {
                    pushedEntities = entities;
                    lastType = PushedValueType.Entities;
                    OnValueReceived(new ValueReceivedEventArgs(entities));
                }
            }
        }

        public void PushPoint(Point point)
        {
            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Point))
                {
                    pushedPoint = point;
                    lastType = PushedValueType.Point;
                    LastPoint = point;
                    OnValueReceived(new ValueReceivedEventArgs(point));
                }
            }
        }

        public void PushText(string text)
        {
            lock (inputGate)
            {
                if (AllowedInputTypes.HasFlag(InputType.Text))
                {
                    pushedString = text;
                    lastType = PushedValueType.Text;
                    OnValueReceived(new ValueReceivedEventArgs(pushedString, InputType.Text));
                }
            }
        }

        public InputType AllowedInputTypes { get; private set; }

        public IEnumerable<string> AllowedDirectives
        {
            get
            {
                if (currentDirective == null)
                {
                    return new string[0];
                }
                else
                {
                    return currentDirective.AllowableDirectives;
                }
            }
        }

        private void SetPrompt(string prompt)
        {
            OnPromptChanged(new PromptChangedEventArgs(prompt));
        }

        public event PromptChangedEventHandler PromptChanged;

        protected virtual void OnPromptChanged(PromptChangedEventArgs e)
        {
            if (PromptChanged != null)
                PromptChanged(this, e);
        }

        public void Reset()
        {
            AllowedInputTypes = InputType.Command;
            SetPrompt("Command");
        }

        public event ValueRequestedEventHandler ValueRequested;

        protected virtual void OnValueRequested(ValueRequestedEventArgs e)
        {
            AllowedInputTypes = e.InputType;
            if (ValueRequested != null)
                ValueRequested(this, e);
        }

        public event ValueReceivedEventHandler ValueReceived;

        protected virtual void OnValueReceived(ValueReceivedEventArgs e)
        {
            _workspace.DebugService.Add(new InputServiceLogEntry(e.InputType, e.Value));

            // write out received value
            switch (e.InputType)
            {
                case InputType.Point:
                    _workspace.OutputService.WriteLine(_workspace.Format(e.Point));
                    break;
                case InputType.Text:
                    _workspace.OutputService.WriteLine(e.Text);
                    break;
            }

            if (pushValueDone != null)
                pushValueDone.SetResult(true);
            if (ValueReceived != null)
                ValueReceived(this, e);
        }

        public event EventHandler InputCanceled;

        protected virtual void OnInputCanceled(EventArgs e)
        {
            var cancel = InputCanceled;
            if (cancel != null)
                cancel(this, e);
        }

        private static Regex relativePoint = new Regex(string.Format("^@{0},{0},{0}$", Point.NumberPattern));

        private static Regex relativeAngle = new Regex(string.Format("^.*<{0}$", Point.NumberPattern));

        public bool TryParsePoint(string text, Point currentCursor, Point lastPoint, out Point point)
        {
            return TryParsePointHelper(text, currentCursor, lastPoint, out point);
        }

        // static to ensure there are no calls to LastPoint, Workspace, etc.
        private static bool TryParsePointHelper(string text, Point currentCursor, Point lastPoint, out Point point)
        {
            var result = false;
            point = default(Point);

            // if only 2 coordinates given
            if (text.ToCharArray().Count(c => c == ',') == 1)
                text += ",0";

            double value = 0.0;
            if (DrawingSettings.TryParseUnits(text, out value))
            {
                // length on current vector
                var length = value;
                var cursor = currentCursor;
                var vec = cursor - lastPoint;
                if (vec.LengthSquared == 0.0)
                {
                    // if no change, report nothing
                }
                else
                {
                    vec = vec.Normalize() * length;
                    point = lastPoint + vec;
                    result = true;
                }
            }
            else if (relativePoint.IsMatch(text))
            {
                // offset from last point
                var offset = Point.Parse(text.Substring(1));
                point = lastPoint + offset;
                result = true;
            }
            else if (relativeAngle.IsMatch(text))
            {
                // distance and angle
                var parts = text.Split("<".ToCharArray(), 2);
                if (DrawingSettings.TryParseUnits(parts[0], out value))
                {
                    var dist = value;
                    var angle = double.Parse(parts[1], NumberStyles.Float, CultureInfo.CurrentCulture);
                    var radians = angle * MathHelper.DegreesToRadians;
                    var offset = new Vector(Math.Cos(radians), Math.Sin(radians), 0) * dist;
                    point = lastPoint + offset;
                    result = true;
                }
                else
                {
                    // distance was invalid
                }
            }
            else if (Point.PointPattern.IsMatch(text))
            {
                // absolute point
                point = Point.Parse(text);
                result = true;
            }
            else
            {
                // invalid point
            }

            return result;
        }
    }
}
