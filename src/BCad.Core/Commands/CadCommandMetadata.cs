// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BCad.Commands
{
    public class CadCommandMetadata
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> CommandAliases { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifier { get; set; }
    }
}
