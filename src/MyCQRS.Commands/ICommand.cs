using System;

namespace MyCQRS.Commands
{
    public interface ICommand
    {
        Guid AggregateId { get; }
    }
}
