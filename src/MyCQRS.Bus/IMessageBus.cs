using System.Collections.Generic;

namespace MyCQRS.Bus
{
    public interface IMessageBus : IUnitOfWork
    {
        void Publish(object message);
        void Publish(IEnumerable<object> messages);
    }
}