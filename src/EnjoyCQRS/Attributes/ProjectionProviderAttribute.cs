using System;

namespace EnjoyCQRS.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProjectionProviderAttribute : Attribute
    {
        public Type Provider { get; }

        public ProjectionProviderAttribute(Type provider)
        {
            Provider = provider;
        }
    }
}
