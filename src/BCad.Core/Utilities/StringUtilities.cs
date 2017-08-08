// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IxMilia.BCad.Utilities
{
    public static class StringUtilities
    {
        public static string NextUniqueName(string baseName, IEnumerable<string> existingNames)
        {
            var existing = new HashSet<string>(existingNames);
            int suffix = 1;
            while (existing.Contains(baseName + suffix))
                suffix++;
            return baseName + suffix;
        }
    }
}
