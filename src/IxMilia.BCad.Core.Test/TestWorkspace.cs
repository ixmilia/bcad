// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading.Tasks;

namespace IxMilia.BCad.Core.Test
{
    [Export(typeof(IWorkspace)), Shared]
    public class TestWorkspace : WorkspaceBase
    {
        public override Task<UnsavedChangesResult> PromptForUnsavedChanges()
        {
            return Task.FromResult(UnsavedChangesResult.Saved);
        }
    }
}
