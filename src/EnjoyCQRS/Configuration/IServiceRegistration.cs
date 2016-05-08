using System;

namespace EnjoyCQRS.Configuration
{
    public interface IServiceRegistration
    {
        void Register(Type type, Lifetime lifetime = Lifetime.Transient);
    }
}