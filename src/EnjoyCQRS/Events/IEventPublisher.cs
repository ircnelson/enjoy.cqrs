using System.Collections.Generic;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.Events
{
    public interface IEventPublisher : ITransactionalMessageBus
    {
        /// <summary>
        /// Publishes the event to the handler(s).
        /// </summary>
        void Publish<TEvent>(TEvent message) where TEvent : IDomainEvent;

        /// <summary>
        /// Publishes the events to the handler(s).
        /// </summary>
        void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent;
    }
}