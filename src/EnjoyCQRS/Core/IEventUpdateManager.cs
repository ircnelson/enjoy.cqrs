using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Core
{
    public interface IEventUpdateManager
    {
        IEnumerable<IDomainEvent> Update(IEnumerable<IDomainEvent> events);
    }
}