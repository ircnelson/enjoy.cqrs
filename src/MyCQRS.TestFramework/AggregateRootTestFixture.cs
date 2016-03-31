using System;
using System.Collections.Generic;
using MyCQRS.Events;
using MyCQRS.EventStore;

namespace MyCQRS.TestFramework
{
    public abstract class AggregateRootTestFixture<TAggregateRoot> where TAggregateRoot : IAggregate, new()
    {
        protected TAggregateRoot AggregateRoot;
        protected Exception CaughtException;
        protected IEnumerable<IDomainEvent> PublishedEvents;
        protected virtual IEnumerable<IDomainEvent> Given()
        {
            return new List<IDomainEvent>();
        }
        protected virtual void Finally() { }
        protected abstract void When();
        
        protected AggregateRootTestFixture()
        {   
            CaughtException = new ThereWasNoExceptionButOneWasExpectedException();
            AggregateRoot = new TAggregateRoot();
            AggregateRoot.LoadFromHistory(Given());

            try
            {
                When();
                PublishedEvents = AggregateRoot.UncommitedEvents;
            }
            catch (Exception exception)
            {
                CaughtException = exception;
            }
            finally
            {
                Finally();
            }
        }
    }
}