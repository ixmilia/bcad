﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.FilePlotters
{
    public class FilePlotterMetadata : IFilePlotterMetadata
    {
        public string DisplayName { get; set; }

        public IEnumerable<string> FileExtensions { get; set; }
    }
}
