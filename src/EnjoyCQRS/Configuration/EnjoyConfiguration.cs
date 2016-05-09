using System.Linq;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Configuration
{
    public class EnjoyConfiguration
    {
        private readonly IScopeResolver _scopeResolver;
        private readonly HandlerDictionary _handlerDictionary;
        private readonly IEnjoyTypeScanner _scanner;

        public EnjoyConfiguration(
            IScopeResolver scopeScopeResolver, 
            HandlerDictionary handlerDictionary, 
            IEnjoyTypeScanner scanner)
        {
            _scopeResolver = scopeScopeResolver;
            _handlerDictionary = handlerDictionary;
            _scanner = scanner;
        }

        public void Setup()
        {
            var resolver = _scopeResolver.BeginScope();
            
            var commands = HandlerHelper.Get<ICommand>(_scanner);
            CommandWrapper.Wrap(resolver, _handlerDictionary, commands);

            var events = HandlerHelper.Get<IDomainEvent>(_scanner);
            EventWrapper.Wrap(resolver, _handlerDictionary, events);
        }
    }
}