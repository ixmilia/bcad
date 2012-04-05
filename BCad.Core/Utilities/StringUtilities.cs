using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Utilities
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
