// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IxMilia.BCad
{
    public class UserDirective
    {
        public string Prompt { get; private set; }

        public IEnumerable<string> AllowableDirectives { get; private set; }

        public UserDirective(string prompt, params string[] allowableDirectives)
        {
            Prompt = prompt;
            AllowableDirectives = allowableDirectives;
        }
    }
}
