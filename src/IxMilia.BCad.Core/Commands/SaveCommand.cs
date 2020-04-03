// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("File.Save", "SAVE", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await workspace.FileSystemService.GetFileNameFromUserForSave();
                if (string.IsNullOrEmpty(fileName))
                    return false;
            }

            var stream = await workspace.FileSystemService.GetStreamForWriting(fileName);
            if (!await workspace.ReaderWriterService.TryWriteDrawing(fileName, drawing, workspace.ActiveViewPort, stream, preserveSettings: true))
                return false;

            SaveAsCommand.UpdateDrawingFileName(workspace, fileName);

            return true;
        }
    }
}
