// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.BCad.UI
{
    public class BCadDialogResult<TResult>
    {
        private TResult result;

        public bool HasValue { get; private set; }

        public TResult Result
        {
            get
            {
                if (!HasValue)
                    throw new NotSupportedException("There is no value set");
                return this.result;
            }
            set
            {
                this.result = value;
                this.HasValue = true;
            }
        }

        public BCadDialogResult()
        {
            this.HasValue = false;
        }

        public BCadDialogResult(TResult result)
        {
            this.HasValue = true;
            Result = result;
        }
    }
}
