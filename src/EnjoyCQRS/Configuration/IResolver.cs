using System;

namespace EnjoyCQRS.Configuration
{
    public interface IResolver
    {
        TService Resolve<TService>();
        object Resolve(Type type);
    }
}