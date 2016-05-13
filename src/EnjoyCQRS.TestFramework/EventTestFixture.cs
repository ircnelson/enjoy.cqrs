using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using Moq;

namespace EnjoyCQRS.TestFramework
{
    public abstract class EventTestFixture<TEvent, TEventHandler>
        where TEvent : class, IDomainEvent 
        where TEventHandler : class, IEventHandler<TEvent>
    {
        private IDictionary<Type, object> mocks;

        protected Exception CaughtException;
        protected TEventHandler EventHandler;
        protected virtual void SetupDependencies() { }
        protected abstract TEvent When();
        protected virtual void Finally() { }

        protected EventTestFixture()
        {
            mocks = new Dictionary<Type, object>();
            CaughtException = new ThereWasNoExceptionButOneWasExpectedException();
            EventHandler = BuildHandler();
            SetupDependencies();

            try
            {
                EventHandler.ExecuteAsync(When());
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
            if (!mocks.ContainsKey(typeof(TType)))
                throw new Exception($"The event handler '{typeof (TEventHandler).FullName}' does not have a dependency upon '{typeof (TType).FullName}'");

            return (Mock<TType>)mocks[typeof(TType)];
        }

        private TEventHandler BuildHandler()
        {
            var constructorInfo = typeof(TEventHandler).GetConstructors().First();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                if (parameter.ParameterType.Name.EndsWith("Repository"))
                {
                    var mockType = typeof(Mock<>).MakeGenericType(parameter.ParameterType);
                    
                    var repositoryMock = (Mock) Activator.CreateInstance(mockType);
                    mocks.Add(parameter.ParameterType, repositoryMock);
                    continue;
                }

                mocks.Add(parameter.ParameterType, CreateMock(parameter.ParameterType));
            }

            return (TEventHandler)constructorInfo.Invoke(mocks.Values.Select(x => ((Mock)x).Object).ToArray());
        }

        private static object CreateMock(Type type)
        {
            var constructorInfo = typeof(Mock<>).MakeGenericType(type).GetConstructors().First();
            return constructorInfo.Invoke(new object[] { });
        }
    }
}