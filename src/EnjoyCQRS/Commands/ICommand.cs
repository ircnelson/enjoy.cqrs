using System;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}
