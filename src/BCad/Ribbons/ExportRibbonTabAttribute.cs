// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Windows.Controls.Ribbon;

namespace IxMilia.BCad.Ribbons
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportRibbonTabAttribute : ExportAttribute, IRibbonTabMetadata
    {
        public ExportRibbonTabAttribute(string id)
            : base(typeof(RibbonTab))
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}
