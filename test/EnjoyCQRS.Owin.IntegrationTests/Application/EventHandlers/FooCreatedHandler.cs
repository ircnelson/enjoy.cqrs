using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.EventHandlers
{
    public class FooCreatedHandler : IEventHandler<FooCreated>
    {
        public Task ExecuteAsync(FooCreated @event)
        {
            return Task.CompletedTask;
        }
    }
}
