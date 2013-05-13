﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.EventArguments;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Services
{
    [Export(typeof(IInputService))]
    internal class InputService : IInputService, IPartImportsSatisfiedNotification
    {
        [Import]
        private IWorkspace Workspace = null;

        public InputService()
        {
            PrimitiveGenerator = null;
            LastPoint = Point.Origin;
            Reset();
        }

        public void OnImportsSatisfied()
        {
            Workspace.CommandExecuted += Workspace_CommandExecuted;
        }

        void Workspace_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            Workspace.SelectedEntities.Clear();
            SetPrompt("Command");
        }

        public void WriteLine(string text)
        {
            OnLineWritten(new WriteLineEventArgs(text));
        }

        public void WriteLine(string text, params object[] param)
        {
            WriteLine(string.Format(text, param));
        }

        public void WriteLineDebug(string text)
        {
            if (Workspace.SettingsManager.Debug)
            {
                WriteLine(text);
            }
        }

        public void WriteLineDebug(string text, params object[] param)
        {
            if (Workspace.SettingsManager.Debug)
            {
                WriteLine(string.Format(text, param));
            }
        }

        private object callbackGate = new object();

        private RubberBandGenerator primitiveGenerator = null;

        public RubberBandGenerator PrimitiveGenerator
        {
            get { return primitiveGenerator; }
            private set
            {
                primitiveGenerator = value;
                OnRubberBandGeneratorChanged(new RubberBandGeneratorChangedEventArgs(primitiveGenerator));
            }
        }

        public Point LastPoint { get; private set; }

        public bool IsDrawing { get { return this.PrimitiveGenerator != null; } }

        private UserDirective currentDirective = null;

        public async Task<ValueOrDirective<double>> GetDistance(string prompt = null, double? defaultDistance = null)
        {
            OnValueRequested(new ValueRequestedEventArgs(InputType.Distance | InputType.Point));
            if (prompt == null)
            {
                prompt = "Offset distance or first point";
            }

            if (defaultDistance.HasValue)
            {
                prompt += string.Format(" [{0}]", defaultDistance.Value);
            }

            await WaitFor(InputType.Distance | InputType.Point, new UserDirective(prompt), null);
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
                case PushedValueType.Point:
                    var first = pushedPoint;
                    ResetWaiters();
                    var second = await GetPoint(new UserDirective("Second point of offset distance"), p =>
                        {
                            return new[] { new PrimitiveLine(first, p) };
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

        public async Task<ValueOrDirective<Point>> GetPoint(UserDirective directive, RubberBandGenerator onCursorMove = null, Point lastPoint = null)
        {
            this.LastPoint = lastPoint ?? this.LastPoint;
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

        public async Task<ValueOrDirective<IEnumerable<Entity>>> GetEntities(string prompt = null, RubberBandGenerator onCursorMove = null)
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
                        Debug.Fail("TODO: allow 'all' directive and 'r' to remove objects from selection");
                        break;
                    case PushedValueType.None:
                        result = ValueOrDirective<IEnumerable<Entity>>.GetValue(entities);
                        awaitingMore = false;
                        break;
                    case PushedValueType.Entity:
                        entities.Add(pushedEntity.Entity);
                        Workspace.SelectedEntities.Add(pushedEntity.Entity);
                        // TODO: print status
                        break;
                    case PushedValueType.Entities:
                        foreach (var e in pushedEntities)
                        {
                            entities.Add(e);
                            Workspace.SelectedEntities.Add(e);
                        }
                        // TODO: print status
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
            PrimitiveGenerator = onCursorMove;
            return pushValueDone.Task;
            //return new Task((Action)(() =>
            //    {
            //        SetPrompt(directive.Prompt);
            //        currentDirective = directive;
            //        AllowedInputTypes = type;
            //        lastType = PushedValueType.None;
            //        pushedPoint = default(Point);
            //        pushedEntity = null;
            //        pushedDirective = null;
            //        pushedString = null;
            //        PrimitiveGenerator = onCursorMove;
            //    }));
        }

        private void ResetWaiters()
        {
            AllowedInputTypes = InputType.Command;
            lastType = PushedValueType.None;
            pushedPoint = default(Point);
            pushedEntity = null;
            pushedDirective = null;
            pushedString = null;
            PrimitiveGenerator = null;
            currentDirective = null;
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
                Workspace.ExecuteCommand(commandName);
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
                        WriteLine("Bad value or directive '{0}'", directive);
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
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

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
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

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

        public event WriteLineEventHandler LineWritten;

        protected virtual void OnLineWritten(WriteLineEventArgs e)
        {
            if (LineWritten != null)
                LineWritten(this, e);
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
            // write out received value
            switch (e.InputType)
            {
                case InputType.Point:
                    WriteLine(e.Point.ToString());
                    break;
                case InputType.Text:
                    WriteLine(e.Text);
                    break;
            }

            if (pushValueDone != null)
                pushValueDone.SetResult(true);
            if (ValueReceived != null)
                ValueReceived(this, e);
        }

        public event RubberBandGeneratorChangedEventHandler RubberBandGeneratorChanged;

        protected virtual void OnRubberBandGeneratorChanged(RubberBandGeneratorChangedEventArgs e)
        {
            if (RubberBandGeneratorChanged != null)
                RubberBandGeneratorChanged(this, e);
        }

        private static Regex relativePoint = new Regex(string.Format("^@{0},{0},{0}$", Point.NumberPattern), RegexOptions.Compiled);

        private static Regex relativeAngle = new Regex(string.Format("^.*<{0}$", Point.NumberPattern), RegexOptions.Compiled);

        public bool TryParsePoint(string text, Point currentCursor, Point lastPoint, out Point point)
        {
            return TryParsePointHelper(text, currentCursor, lastPoint, out point);
        }

        // static to ensure there are no calls to LastPoint, Workspace, etc.
        private static bool TryParsePointHelper(string text, Point currentCursor, Point lastPoint, out Point point)
        {
            // if only 2 coordinates given
            if (text.Count(c => c == ',') == 1)
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
                    // if no change report the last point
                    point = lastPoint;
                }
                else
                {
                    vec = vec.Normalize() * length;
                    point = lastPoint + vec;
                }
            }
            else if (relativePoint.IsMatch(text))
            {
                // offset from last point
                var offset = Point.Parse(text.Substring(1));
                point = lastPoint + offset;
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
                }
                else
                {
                    // distance was invalid
                    point = null;
                }
            }
            else if (Point.PointPattern.IsMatch(text))
            {
                // absolute point
                point = Point.Parse(text);
            }
            else
            {
                // invalid point
                point = null;
            }

            return point != null;
        }
    }
}
