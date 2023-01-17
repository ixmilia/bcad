using System.Threading.Tasks;

namespace IxMilia.BCad.Commands {
    public class ExplodeCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var entitiesToExplodeResult = await workspace.InputService.GetEntities("Select entities to explode");
            if (entitiesToExplodeResult.Cancel || !entitiesToExplodeResult.HasValue)
            {
                return false;
            }

            var updatedDrawing = workspace.Drawing.ExplodeEntities(entitiesToExplodeResult.Value);
            workspace.Update(drawing: updatedDrawing);
            return true;
        }
    }
}
