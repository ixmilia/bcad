// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace BCad.Services
{
    public interface IFileSystemService : IWorkspaceService
    {
        Task<string> GetFileNameFromUserForSave();
        Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications);
        Task<string> GetFileNameFromUserForOpen();
        Task<bool> TryWriteDrawing(string fileName, Drawing drawing, ViewPort viewPort);
        Task<bool> TryReadDrawing(string fileName, out Drawing drawing, out ViewPort viewPort);
    }
}
