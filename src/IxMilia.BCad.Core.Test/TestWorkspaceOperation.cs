using System.Collections.Generic;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.Core.Test
{
    public abstract class TestWorkspaceOperation
    {
        public abstract void DoOperation(IWorkspace workspace);
    }

    public class PushNoneOperation : TestWorkspaceOperation
    {
        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushNone();
    }

    public class PushCommandOperation : TestWorkspaceOperation
    {
        public string Command { get; }

        public PushCommandOperation(string command)
        {
            Command = command;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushCommand(Command);
    }

    public class PushDistanceOperation : TestWorkspaceOperation
    {
        public double Distance { get; }

        public PushDistanceOperation(double distance)
        {
            Distance = distance;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushDistance(Distance);
    }

    public class PushDirectiveOperation : TestWorkspaceOperation
    {
        public string Directive { get; }

        public PushDirectiveOperation(string directive)
        {
            Directive = directive;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushDirective(Directive);
    }

    public class PushPointOperation : TestWorkspaceOperation
    {
        public Point Point { get; }

        public PushPointOperation(Point point)
        {
            Point = point;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushPoint(Point);
    }

    public class PushEntityOperation : TestWorkspaceOperation
    {
        public SelectedEntity Entity { get; }

        public PushEntityOperation(SelectedEntity entity)
        {
            Entity = entity;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushEntity(Entity);
    }

    public class PushEntitiesOperation : TestWorkspaceOperation
    {
        public IEnumerable<Entity> Entities { get; }

        public PushEntitiesOperation(IEnumerable<Entity> entities)
        {
            Entities = entities;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushEntities(Entities);
    }

    public class PushTextOperation : TestWorkspaceOperation
    {
        public string Text { get; }

        public PushTextOperation(string text)
        {
            Text = text;
        }

        public override void DoOperation(IWorkspace workspace) => workspace.InputService.PushText(Text);
    }
}
