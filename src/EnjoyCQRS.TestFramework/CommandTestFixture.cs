using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using Moq;

namespace EnjoyCQRS.TestFramework
{
    public abstract class CommandTestFixture<TCommand, TCommandHandler, TAggregateRoot>
        where TCommand : class, ICommand
        where TCommandHandler : class, ICommandHandler<TCommand>
        where TAggregateRoot : Aggregate, new()
    {
        private readonly IDictionary<Type, object> _mocks;

        protected TAggregateRoot AggregateRoot;
        protected TCommandHandler CommandHandler;
        protected Exception CaughtException;
        protected IEnumerable<IDomainEvent> PublishedEvents;
        protected virtual void SetupDependencies() { }
        protected virtual IEnumerable<IDomainEvent> Given()
        {
            return new List<IDomainEvent>();
        }
        protected virtual void Finally() { }
        protected abstract TCommand When();

        protected CommandTestFixture()
        {
            _mocks = new Dictionary<Type, object>();
            CaughtException = new ThereWasNoExceptionButOneWasExpectedException();
            AggregateRoot = new TAggregateRoot();
            AggregateRoot.LoadFromHistory(Given());

            CommandHandler = BuildHandler();

            SetupDependencies();

            try
            {
                CommandHandler.ExecuteAsync(When());
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
            return (Mock<TType>)_mocks[typeof(TType)];
        }

        private TCommandHandler BuildHandler()
        {
            var constructorInfo = typeof(TCommandHandler).GetConstructors().First();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                if (parameter.ParameterType == typeof(IRepository))
                {
                    var repositoryMock = new Mock<IRepository>();
                    repositoryMock.Setup(x => x.GetByIdAsync<TAggregateRoot>(It.IsAny<Guid>())).Returns(Task.FromResult(AggregateRoot));
                    repositoryMock.Setup(x => x.AddAsync(It.IsAny<TAggregateRoot>())).Callback<TAggregateRoot>(x => AggregateRoot = x);
                    _mocks.Add(parameter.ParameterType, repositoryMock);
                    continue;
                }

                _mocks.Add(parameter.ParameterType, CreateMock(parameter.ParameterType));
            }

            return (TCommandHandler)constructorInfo.Invoke(_mocks.Values.Select(x => ((Mock)x).Object).ToArray());
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
        public static EventVersionSetter Set(IDomainEvent domainEvent)
        {
            return new EventVersionSetter(domainEvent);
        }
    }

    public class EventVersionSetter
    {
        private readonly IDomainEvent _domainEvent;

        public EventVersionSetter(IDomainEvent domainEvent)
        {
            _domainEvent = domainEvent;
        }

        public IDomainEvent ToVersion(int version)
        {
            if (_domainEvent is DomainEvent)
            {
                ((DomainEvent) _domainEvent).Version = version;
            }

            return _domainEvent;
        }
    }
}