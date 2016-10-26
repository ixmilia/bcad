// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;

namespace BCad.Services
{
    [ExportWorkspaceService, Shared]
    internal class DebugService : IDebugService
    {
        private List<LogEntry> entries;

        [ImportingConstructor]
        public DebugService()
        {
            entries = new List<LogEntry>();
        }

        public void Add(LogEntry entry)
        {
            entries.Add(entry);
        }

        public LogEntry[] GetLog()
        {
            return entries.ToArray();
        }
    }
}
