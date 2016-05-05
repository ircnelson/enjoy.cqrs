using System;

namespace EnjoyCQRS.Bus
{
    public interface IRegisterHandler
    {
        void Register<TMessage>(Action<TMessage> route) where TMessage : class;
    }
}