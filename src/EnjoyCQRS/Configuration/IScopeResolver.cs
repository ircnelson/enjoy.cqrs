using System;

namespace EnjoyCQRS.Configuration
{
    public interface IScopeResolver : IResolver, IDisposable
    {
        IScopeResolver BeginScope();
    }
}