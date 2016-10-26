// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;

namespace BCad.Services
{
    public class ExportWorkspaceServiceAttribute : ExportAttribute
    {
        public ExportWorkspaceServiceAttribute()
            : base(typeof(IWorkspaceService))
        {
        }
    }
}
