using System;
using System.Collections.Generic;
using System.Text;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Core.Stubs
{
    class DefaultEventUpdateManager : IEventUpdateManager
    {
        public IEnumerable<IDomainEvent> Update(IEnumerable<IDomainEvent> events)
        {
            yield return null;
        }
    }
}
