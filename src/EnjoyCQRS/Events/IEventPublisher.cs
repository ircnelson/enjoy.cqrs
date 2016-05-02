using System.Collections.Generic;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.Events
{
    public interface IEventPublisher : IUnitOfWork
    {
        void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent;
    }
}