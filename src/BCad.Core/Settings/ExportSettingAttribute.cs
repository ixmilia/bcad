// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;

namespace BCad.Settings
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportSettingAttribute : ExportAttribute, ISettingMetadata
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }

        public ExportSettingAttribute(string name, Type type, object value)
            : base(typeof(object))
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
