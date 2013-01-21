using System;
using System.Collections.Generic;
using BCad.Entities;
using BCad.Services;

namespace BCad.EventArguments
{
    public class ValueReceivedEventArgs : EventArgs
    {
        public InputType InputType { get; private set; }

        private double distance;
        private Point point;
        private SelectedEntity entity;
        private IEnumerable<Entity> entities;
        private string text;
        private string directive;
        private string command;

        public double Distance
        {
            get
            {
                if (InputType != InputType.Distance)
                    throw new Exception("Value was not a distance");
                return distance;
            }
        }

        public Point Point
        {
            get
            {
                if (InputType != InputType.Point)
                    throw new Exception("Value was not a point");
                return point;
            }
        }

        public SelectedEntity Entity
        {
            get
            {
                if (InputType != InputType.Entity)
                    throw new Exception("Value was not an entity");
                return entity;
            }
        }

        public IEnumerable<Entity> Entities
        {
            get
            {
                if (InputType != InputType.Entities)
                    throw new Exception("Value was not an entity collection");
                return entities;
            }
        }

        public string Text
        {
            get
            {
                if (InputType != InputType.Text)
                    throw new Exception("Value was not text");
                return text;
            }
        }

        public string Directive
        {
            get
            {
                if (InputType != InputType.Directive)
                    throw new Exception("Value was not a directive");
                return directive;
            }
        }

        public string Command
        {
            get
            {
                if (InputType != InputType.Command)
                    throw new Exception("Value was not a command");
                return command;
            }
        }

        public ValueReceivedEventArgs()
        {
            InputType = InputType.None;
        }

        public ValueReceivedEventArgs(double distance)
        {
            this.distance = distance;
            InputType = InputType.Distance;
        }

        public ValueReceivedEventArgs(Point point)
        {
            this.point = point;
            InputType = InputType.Point;
        }

        public ValueReceivedEventArgs(SelectedEntity entity)
        {
            this.entity = entity;
            InputType = InputType.Entity;
        }

        public ValueReceivedEventArgs(IEnumerable<Entity> entities)
        {
            this.entities = entities;
            InputType = InputType.Entities;
        }

        public ValueReceivedEventArgs(string value, InputType type)
        {
            switch (type)
            {
                case InputType.Command:
                    command = value;
                    break;
                case InputType.Directive:
                    directive = value;
                    break;
                case Services.InputType.Text:
                    text = value;
                    break;
                default:
                    throw new Exception("Unacceptable type");
            }
            InputType = type;
        }
    }
}
