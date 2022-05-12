using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;

namespace IxMilia.BCad.Commands
{
    public class LineTypeCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var parameter = new LineTypeDialogParameters(workspace.Drawing);
            var result = (LineTypeDialogResult)await workspace.DialogService.ShowDialog("line-type", parameter);
            if (result == null)
            {
                return false;
            }

            var lineTypes = workspace.Drawing.LineTypes;
            var finalOldLineTypeNames = new HashSet<string>(result.LineTypes.Select(l => l.OldLineTypeName));
            var lineTypesToDelete = lineTypes.GetValues().Where(l => !finalOldLineTypeNames.Contains(l.Name));
            foreach (var LineTypeToDelete in lineTypesToDelete)
            {
                lineTypes = lineTypes.Delete(LineTypeToDelete.Name);
            }

            foreach (var lineTypeResult in result.LineTypes)
            {
                if (lineTypes.TryFind(lineTypeResult.OldLineTypeName, out var lineType))
                {
                    // update
                    lineType = lineType.Update(
                        name: lineTypeResult.NewLineTypeName,
                        pattern: lineTypeResult.Pattern,
                        description: lineTypeResult.Description);
                }
                else
                {
                    // add
                    lineType = new LineType(
                        name: lineTypeResult.NewLineTypeName,
                        pattern: lineTypeResult.Pattern,
                        description: lineTypeResult.Description);
                }

                lineTypes = lineTypes.Insert(lineType.Name, lineType);
            }

            workspace.Update(drawing: workspace.Drawing.Update(lineTypes: lineTypes));
            return true;
        }
    }
}
