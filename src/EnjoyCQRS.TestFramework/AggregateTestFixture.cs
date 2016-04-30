using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventStore;

namespace EnjoyCQRS.TestFramework
{
    public abstract class AggregateTestFixture<TAggregateRoot> where TAggregateRoot : Aggregate, new()
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
        
        protected AggregateTestFixture()
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