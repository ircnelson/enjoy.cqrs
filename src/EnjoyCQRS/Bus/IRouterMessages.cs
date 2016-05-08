using System;

namespace EnjoyCQRS.Bus
{
    public interface IRouterMessages
    {
        void Register<TMessage>(Action<TMessage> route) where TMessage : class;
        void Route(object message);
    }
}