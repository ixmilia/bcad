// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.EventArguments;

namespace IxMilia.BCad.Services
{
    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs e);

    public interface IOutputService : IWorkspaceService
    {
        event WriteLineEventHandler LineWritten;
        void WriteLine(string text, params object[] args);
    }
}
