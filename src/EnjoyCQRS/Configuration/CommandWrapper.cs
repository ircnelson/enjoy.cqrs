using System;
using System.Collections.Generic;
using System.Reflection;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Configuration
{
    internal class CommandWrapper
    {
        private readonly IResolver _resolver;
        private readonly HandlerDictionary _enjoyHandlers;
        private readonly IEnjoyTypeScanner _scanner;
        private static MethodInfo _createPublishActionWrappedInTransactionMethod;
        private static MethodInfo _registerMethod;
        
        public CommandWrapper(IResolver resolver, HandlerDictionary enjoyHandlers, IEnjoyTypeScanner scanner)
        {
            _resolver = resolver;
            _enjoyHandlers = enjoyHandlers;
            _scanner = scanner;
        }

        public static void Wrap(IResolver resolver, HandlerDictionary enjoyHandlers, IEnjoyTypeScanner scanner)
        {
            var wrapper = new CommandWrapper(resolver, enjoyHandlers, scanner);
            wrapper.Register();
        }

        public void Register()
        {
            var registerHandler = _resolver.Resolve<IRegisterHandler>();

            _createPublishActionWrappedInTransactionMethod = GetType().GetMethod(nameof(CreatePublishActionWrappedInTransaction), BindingFlags.Instance | BindingFlags.NonPublic);
            _registerMethod = registerHandler.GetType().GetMethod(nameof(IRegisterHandler.Register));

            var dtos = HandlerHelper.Get<ICommand>(_scanner);

            foreach (var dto in dtos)
            {
                IList<Type> handlerTypes;
                if (!_enjoyHandlers.TryGetValue(new HandlerMetadata(dto, HandlerType.Command), out handlerTypes))
                    throw new Exception($"No handlers found for '{dto.FullName}'");

                foreach (var handler in handlerTypes)
                {
                    var injectedHandler = _resolver.Resolve(handler);

                    var action = CreateTheProperAction(dto, injectedHandler);
                    RegisterTheCreatedActionWithTheMessageRouter(registerHandler, dto, action);
                }
            }
        }

        private static void RegisterTheCreatedActionWithTheMessageRouter(IRegisterHandler registerHandler, Type commandType, object action)
        {
            _registerMethod.MakeGenericMethod(commandType).Invoke(registerHandler, new[] { action });
        }

        private object CreateTheProperAction(Type commandType, object commandHandler)
        {
            return _createPublishActionWrappedInTransactionMethod.MakeGenericMethod(commandType, commandHandler.GetType()).Invoke(this, new[] { commandHandler });
        }

        private Action<TCommand> CreatePublishActionWrappedInTransaction<TCommand, TCommandHandler>(TCommandHandler commandHandler)
            where TCommand : class, ICommand 
            where TCommandHandler : ICommandHandler<TCommand>
        {
            return command => _resolver.Resolve<TransactionHandler<TCommand, TCommandHandler>>().Execute(command, commandHandler);
        }
    }
}