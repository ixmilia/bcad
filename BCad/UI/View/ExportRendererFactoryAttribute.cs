using System;
using System.Composition;

namespace BCad.UI
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportRendererFactoryAttribute : ExportAttribute
    {
        public string FactoryName { get; private set; }

        public ExportRendererFactoryAttribute(string factoryName)
            : base(typeof(IRendererFactory))
        {
            FactoryName = factoryName;
        }
    }
}
