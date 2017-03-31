using System;
using System.Reflection;
using EnjoyCQRS.EventSource.Projections;

namespace EnjoyCQRS.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProjectionProviderAttribute : Attribute
    {
        public Type Provider { get; }

        public ProjectionProviderAttribute(Type provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            if (provider.IsAssignableFrom(typeof(IProjectionProvider))) throw new ArgumentException($"Provider should be inherited of {nameof(IProjectionProvider)}.");

            Provider = provider;
        }
    }
}
