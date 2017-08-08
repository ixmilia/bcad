// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Services
{
    public abstract class LogEntry
    {
    }

    public interface IDebugService : IWorkspaceService
    {
        void Add(LogEntry entry);
        LogEntry[] GetLog();
    }
}
