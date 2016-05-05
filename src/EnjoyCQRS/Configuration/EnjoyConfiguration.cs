using System;
using System.Linq;

namespace EnjoyCQRS.Configuration
{
    public class EnjoyConfiguration
    {
        private readonly IResolver _resolver;
        private readonly HandlerDictionary _handlerDictionary;
        private readonly IEnjoyTypeScanner _scanner;

        public EnjoyConfiguration(IResolver resolver, HandlerDictionary handlerDictionary, IEnjoyTypeScanner scanner)
        {
            _resolver = resolver;
            _handlerDictionary = handlerDictionary;
            _scanner = scanner;
        }

        public void Setup()
        {
            CommandWrapper.Wrap(_resolver, _handlerDictionary, _scanner);
            EventWrapper.Wrap(_resolver, _handlerDictionary, _scanner);
        }
    }

    public interface IResolver
    {
        TService Resolve<TService>();
        object Resolve(Type type);
    }
}