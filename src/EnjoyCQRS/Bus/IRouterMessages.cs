using System;

namespace EnjoyCQRS.Bus
{
    public interface IRouterMessages
    {
        void Route(object message);
    }
}