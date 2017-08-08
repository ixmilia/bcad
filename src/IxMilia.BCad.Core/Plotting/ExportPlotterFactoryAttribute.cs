// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;

namespace IxMilia.BCad.Plotting
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportPlotterFactoryAttribute : ExportAttribute, IPlotterFactoryMetadata
    {
        public string DisplayName { get; }
        public string ViewTypeName { get; set; }

        public ExportPlotterFactoryAttribute(string displayName)
            : base(typeof(IPlotterFactory))
        {
            DisplayName = displayName;
        }
    }
}
