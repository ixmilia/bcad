// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.Services
{
    public class FileSpecification
    {
        public string DisplayName { get; private set; }

        public IEnumerable<string> FileExtensions { get; private set; }

        public FileSpecification(string displayName, IEnumerable<string> fileExtensions)
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }
}
