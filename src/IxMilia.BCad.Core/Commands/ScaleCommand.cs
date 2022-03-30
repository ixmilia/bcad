using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class ScaleCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            // get all inputs
            var selection = await workspace.InputService.GetEntities("Select entities");
            if (selection.Cancel || !selection.HasValue)
            {
                return false;
            }

            var entities = selection.Value;

            var basePointValue = await workspace.InputService.GetPoint(new UserDirective("Select base point"));
            if (basePointValue.Cancel || !basePointValue.HasValue)
            {
                return false;
            }

            var basePoint = basePointValue.Value;

            var scaleFactorValue = await workspace.InputService.GetDistance("Scale factor");
            if (scaleFactorValue.Cancel || !scaleFactorValue.HasValue)
            {
                return false;
            }

            var scaleFactor = scaleFactorValue.Value;

            // now do it
            var drawing = workspace.Drawing.ScaleEntities(entities, basePoint, scaleFactor);
            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
