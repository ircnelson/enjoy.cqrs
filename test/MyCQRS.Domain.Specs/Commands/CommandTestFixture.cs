using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MyCQRS.CommandHandlers;
using MyCQRS.Commands;
using MyCQRS.Events;

namespace MyCQRS.Domain.Specs.Commands
{
    public abstract class CommandTestFixture<TCommand, TCommandHandler, TAggregateRoot>
        where TCommand : class, ICommand
        where TCommandHandler : class, ICommandHandler<TCommand>
        where TAggregateRoot : Aggregate, new()
    {
        private IDictionary<Type, object> mocks;

        protected TAggregateRoot AggregateRoot;
        protected TCommandHandler CommandHandler;
        protected Exception CaughtException;
        protected IEnumerable<IEvent> PublishedEvents;
        protected virtual void SetupDependencies() { }
        protected virtual IEnumerable<IEvent> Given()
        {
            return new List<IEvent>();
        }
        protected virtual void Finally() { }
        protected abstract TCommand When();

        protected CommandTestFixture()
        {
            mocks = new Dictionary<Type, object>();
            CaughtException = new ThereWasNoExceptionButOneWasExpectedException();
            AggregateRoot = new TAggregateRoot();
            AggregateRoot.LoadFromHistory(Given());

            CommandHandler = BuildCommandHandler();

            SetupDependencies();
            try
            {
                CommandHandler.Execute(When());
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

        public Mock<TType> OnDependency<TType>() where TType : class
        {
            return (Mock<TType>)mocks[typeof(TType)];
        }

        private TCommandHandler BuildCommandHandler()
        {
            var constructorInfo = typeof(TCommandHandler).GetConstructors().First();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                if (parameter.ParameterType == typeof(IDomainRepository))
                {
                    var repositoryMock = new Mock<IDomainRepository>();
                    repositoryMock.Setup(x => x.GetById<TAggregateRoot>(It.IsAny<Guid>())).Returns(AggregateRoot);
                    repositoryMock.Setup(x => x.Add(It.IsAny<TAggregateRoot>())).Callback<TAggregateRoot>(x => AggregateRoot = x);
                    mocks.Add(parameter.ParameterType, repositoryMock);
                    continue;
                }

                mocks.Add(parameter.ParameterType, CreateMock(parameter.ParameterType));
            }

            return (TCommandHandler)constructorInfo.Invoke(mocks.Values.Select(x => ((Mock)x).Object).ToArray());
        }

        private static object CreateMock(Type type)
        {
            var constructorInfo = typeof(Mock<>).MakeGenericType(type).GetConstructors().First();
            return constructorInfo.Invoke(new object[] { });
        }
    }

    public class ThereWasNoExceptionButOneWasExpectedException : Exception { }

    public class PrepareDomainEvent
    {
        public static EventVersionSetter Set(IEvent domainEvent)
        {
            return new EventVersionSetter(domainEvent);
        }
    }

    public class EventVersionSetter
    {
        private readonly IEvent _domainEvent;

        public EventVersionSetter(IEvent domainEvent)
        {
            _domainEvent = domainEvent;
        }

        public IEvent ToVersion(int version)
        {
            //_domainEvent.Version = version;
            return _domainEvent;
        }
    }
}