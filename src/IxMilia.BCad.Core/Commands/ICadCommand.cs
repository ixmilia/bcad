// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public interface ICadCommand
    {
        Task<bool> Execute(IWorkspace workspace, object arg = null);
    }
}
