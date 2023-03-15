using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class SaveCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            string fileName = workspace.Drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await workspace.GetDrawingFilenameFromUserForSave();
                if (string.IsNullOrEmpty(fileName))
                    return false;
            }

            var result = await SaveAsCommand.SaveFile(workspace, fileName, true);
            return result;
        }
    }
}
