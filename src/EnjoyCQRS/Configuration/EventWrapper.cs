using System;
using System.Collections.Generic;
using System.Reflection;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Configuration
{
    internal class EventWrapper
    {
        private readonly IResolver _resolver;
        private readonly HandlerDictionary _enjoyHandlers;
        private readonly IEnjoyTypeScanner _scanner;
        private static MethodInfo _createPublishActionMethod;
        private static MethodInfo _registerMethod;

        public EventWrapper(IResolver resolver, HandlerDictionary enjoyHandlers, IEnjoyTypeScanner scanner)
        {
            _resolver = resolver;
            _enjoyHandlers = enjoyHandlers;
            _scanner = scanner;
        }

        public static void Wrap(IResolver resolver, HandlerDictionary enjoyHandlers, IEnjoyTypeScanner scanner)
        {
            var wrapper = new EventWrapper(resolver, enjoyHandlers, scanner);
            wrapper.Register();
        }

        public void Register()
        {
            var registerHandler = _resolver.Resolve<IRegisterHandler>();

            _createPublishActionMethod = GetType().GetMethod(nameof(CreatePublishAction));
            _registerMethod = registerHandler.GetType().GetMethod(nameof(IRegisterHandler.Register));

            var dtos = HandlerHelper.Get<IDomainEvent>(_scanner);

            foreach (var dto in dtos)
            {
                IList<Type> handlerTypes;
                if (!_enjoyHandlers.TryGetValue(new HandlerMetadata(dto, HandlerType.Event), out handlerTypes))
                    throw new Exception($"No handlers found for '{dto.FullName}'");

                foreach (var handler in handlerTypes)
                {
                    var injectedHandler = _resolver.Resolve(handler);

                    var action = CreateTheProperAction(dto, injectedHandler);
                    RegisterTheCreatedActionWithTheMessageRouter(registerHandler, dto, action);
                }
            }
        }

        private static void RegisterTheCreatedActionWithTheMessageRouter(IRegisterHandler registerHandler, Type eventType, object action)
        {
            _registerMethod.MakeGenericMethod(eventType).Invoke(registerHandler, new[] { action });
        }

        private object CreateTheProperAction(Type eventType, object eventHandler)
        {
            return _createPublishActionMethod.MakeGenericMethod(eventType, eventHandler.GetType()).Invoke(this, new[] { eventHandler });
        }

        public Action<TMessage> CreatePublishAction<TMessage, TMessageHandler>(TMessageHandler messageHandler)
            where TMessage : class, IDomainEvent 
            where TMessageHandler : IEventHandler<TMessage>
        {
            return messageHandler.Execute;
        }
    }
}