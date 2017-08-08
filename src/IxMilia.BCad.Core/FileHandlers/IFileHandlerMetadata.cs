// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace IxMilia.BCad.FileHandlers
{
    public interface IFileHandlerMetadata
    {
        string DisplayName { get; }

        IEnumerable<string> FileExtensions { get; }

        bool CanRead { get; }

        bool CanWrite { get; }
    }
}
