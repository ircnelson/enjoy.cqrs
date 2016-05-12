using System.Collections.Generic;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Messages
{
    public interface IEventPublisher : ITransactionalMessageBus
    {
        /// <summary>
        /// Publishes the event to the handler(s).
        /// </summary>
        void Publish<TEvent>(TEvent message) where TEvent : IDomainEvent;

        /// <summary>
        /// Publishes the event to the handler(s).
        /// </summary>
        void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent;
    }
}