using System.Linq;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Configuration
{
    public class EnjoyConfiguration
    {
        private readonly IResolver _resolver;
        private readonly HandlerDictionary _handlerDictionary;
        private readonly IEnjoyTypeScanner _scanner;

        public EnjoyConfiguration(
            IScopeResolver resolver, 
            HandlerDictionary handlerDictionary, 
            IEnjoyTypeScanner scanner)
        {
            _resolver = resolver;
            _handlerDictionary = handlerDictionary;
            _scanner = scanner;
        }

        public void Setup()
        {
            var commands = HandlerHelper.Get<ICommand>(_scanner);
            CommandWrapper.Wrap(_resolver, _handlerDictionary, commands);

            var events = HandlerHelper.Get<IDomainEvent>(_scanner);
            EventWrapper.Wrap(_resolver, _handlerDictionary, events);
        }
    }
}