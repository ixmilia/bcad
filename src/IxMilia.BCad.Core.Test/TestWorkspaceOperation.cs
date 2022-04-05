using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    public abstract class TestWorkspaceOperation
    {
        public abstract InputType InputType { get; }
    }

    public class PushNoneOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.None;
    }

    public class PushCommandOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Command;

        public string Command { get; }

        public PushCommandOperation(string command)
        {
            Command = command;
        }
    }

    public class PushDistanceOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Distance;

        public double Distance { get; }

        public PushDistanceOperation(double distance)
        {
            Distance = distance;
        }
    }

    public class PushDirectiveOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Directive;

        public string Directive { get; }

        public PushDirectiveOperation(string directive)
        {
            Directive = directive;
        }
    }

    public class PushPointOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Point;

        public Point Point { get; }

        public PushPointOperation(Point point)
        {
            Point = point;
        }
    }

    public class PushEntityOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Entity;

        public SelectedEntity Entity { get; }

        public PushEntityOperation(SelectedEntity entity)
        {
            Entity = entity;
        }
    }

    public class PushEntitiesOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Entities;

        public IEnumerable<Entity> Entities { get; }

        public PushEntitiesOperation(IEnumerable<Entity> entities)
        {
            Entities = entities;
        }
    }

    public class PushTextOperation : TestWorkspaceOperation
    {
        public override InputType InputType => InputType.Text;

        public string Text { get; }

        public PushTextOperation(string text)
        {
            Text = text;
        }
    }
}
