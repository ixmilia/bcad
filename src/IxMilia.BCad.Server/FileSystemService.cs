// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;
using StreamJsonRpc;

namespace IxMilia.BCad.Server
{
    [ExportWorkspaceService, Shared]
    internal class FileSystemService : IFileSystemService
    {
        public JsonRpc Rpc;

        public async Task<string> GetFileNameFromUserForOpen()
        {
            var fileName = await Rpc.InvokeAsync<string>("GetFileNameFromUserForOpen", null);
            return fileName;
        }

        public async Task<string> GetFileNameFromUserForSave()
        {
            var fileName = await Rpc.InvokeAsync<string>("GetFileNameFromUserForSave", null);
            return fileName;
        }

        public Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamForReading(string fileName)
        {
            return Task.FromResult((Stream)File.Open(fileName, FileMode.Open));
        }

        public Task<Stream> GetStreamForWriting(string fileName)
        {
            return Task.FromResult((Stream)File.Open(fileName, FileMode.Create));
        }
    }
}
