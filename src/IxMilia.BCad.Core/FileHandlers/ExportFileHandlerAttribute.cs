// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;

namespace IxMilia.BCad.FileHandlers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportFileHandlerAttribute : ExportAttribute, IFileHandlerMetadata
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public bool CanRead { get; private set; }

        public bool CanWrite { get; private set; }

        public ExportFileHandlerAttribute(string displayName, bool canRead, bool canWrite, params string[] fileExtensions)
            : base(typeof(IFileHandler))
        {
            DisplayName = displayName;
            CanRead = canRead;
            CanWrite = canWrite;
            FileExtensions = fileExtensions;
        }
    }
}
