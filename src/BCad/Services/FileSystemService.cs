// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.FileHandlers;
using Microsoft.Win32;

namespace IxMilia.BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class FileSystemService : IFileSystemService
    {
        [ImportMany]
        public IEnumerable<Lazy<IFileHandler, FileHandlerMetadata>> FileHandlers { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public Task<string> GetFileNameFromUserForSave()
        {
            var x = FileHandlers.Where(fw => fw.Metadata.CanWrite).Select(fw => new FileSpecification(fw.Metadata.DisplayName, fw.Metadata.FileExtensions));
            return GetFileNameFromUserForWrite(x);
        }

        public Task<string> GetFileNameFromUserForWrite(IEnumerable<FileSpecification> fileSpecifications)
        {
            var filter = string.Join("|",
                from fs in fileSpecifications.OrderBy(f => f.DisplayName)
                let exts = string.Join(";", fs.FileExtensions.Select(x => "*" + x))
                select string.Format("{0}|{1}", fs.DisplayName, exts));

            var dialog = new SaveFileDialog();
            dialog.DefaultExt = fileSpecifications.First().FileExtensions.First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
                return Task.FromResult<string>(null);

            return Task.FromResult(dialog.FileName);
        }

        public Task<string> GetFileNameFromUserForOpen()
        {
            var fileSpecifications = FileHandlers.Where(fr => fr.Metadata.CanRead).Select(fr => new FileSpecification(fr.Metadata.DisplayName, fr.Metadata.FileExtensions));
            var filter = string.Join("|",
                    from r in fileSpecifications.OrderBy(f => f.DisplayName)
                    let exts = string.Join(";", r.FileExtensions.Select(x => "*" + x))
                    select string.Format("{0}|{1}", r.DisplayName, exts));

            var all = string.Format("{0}|{1}",
                "All supported types",
                string.Join(";", fileSpecifications.SelectMany(f => f.FileExtensions).Select(x => "*" + x).OrderBy(x => x)));

            filter = string.Join("|", all, filter);

            var dialog = new OpenFileDialog();
            dialog.DefaultExt = fileSpecifications.First().FileExtensions.First();
            dialog.Filter = filter;
            var result = dialog.ShowDialog();
            if (result != true)
                return Task.FromResult<string>(null);
            return Task.FromResult(dialog.FileName);
        }

        public Task<Stream> GetStreamForWriting(string fileName)
        {
            return Task.FromResult((Stream)new FileStream(fileName, FileMode.Create));
        }

        public Task<Stream> GetStreamForReading(string fileName)
        {
            return Task.FromResult((Stream)new FileStream(fileName, FileMode.Open));
        }
    }
}
