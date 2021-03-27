using System.Collections.Generic;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.Commands
{
    public class JoinPolylineCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            var joinEntitiesChoice = await workspace.InputService.GetEntities("Select entities to join");
            if (joinEntitiesChoice.Cancel || !joinEntitiesChoice.HasValue)
            {
                return true;
            }

            var entities = new HashSet<Entity>(joinEntitiesChoice.Value);
            if (entities.Count == 0)
            {
                return true;
            }

            drawing = drawing.CombineEntitiesIntoPolyline(entities, drawing.CurrentLayerName);
            workspace.Update(drawing);
            return true;
        }
    }
}
