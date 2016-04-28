using System;

namespace EnjoyCQRS.Commands
{
    public abstract class Command : ICommand
    {
        public Guid AggregateId { get; }

        protected Command(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}