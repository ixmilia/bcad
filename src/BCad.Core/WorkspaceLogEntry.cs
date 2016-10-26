// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Services;

namespace BCad
{
    internal class WorkspaceLogEntry : LogEntry
    {
        public string Event { get; private set; }

        public WorkspaceLogEntry(string @event)
        {
            Event = @event;
        }

        public override string ToString()
        {
            return string.Format("workspace: {0}", Event);
        }
    }
}
