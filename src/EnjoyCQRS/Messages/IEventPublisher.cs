using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Messages
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes the event.
        /// </summary>
        Task PublishAsync<TEvent>(TEvent message) where TEvent : IDomainEvent;

        /// <summary>
        /// Publishes the event.
        /// </summary>
        Task PublishAsync<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent;

        /// <summary>
        /// Confirm publications.
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();

        /// <summary>
        /// Revert publications.
        /// </summary>
        void Rollback();
    }
}