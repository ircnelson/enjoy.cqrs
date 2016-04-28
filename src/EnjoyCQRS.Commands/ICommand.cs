using System;

namespace EnjoyCQRS.Commands
{
    public interface ICommand
    {
        Guid AggregateId { get; }
    }
}
