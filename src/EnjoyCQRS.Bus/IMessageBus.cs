using System.Collections.Generic;

namespace EnjoyCQRS.Bus
{
    public interface IMessageBus : IUnitOfWork
    {
        void Publish(object message);
        void Publish(IEnumerable<object> messages);
    }
}