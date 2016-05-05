using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Configuration
{
    public class HandlerScanner
    {
        private readonly IEnjoyTypeScanner _enjoyTypeScanner;
        private readonly HandlerDictionary _enjoyCommandHandlers = new HandlerDictionary();
        private readonly HandlerDictionary _enjoyEventHandlers = new HandlerDictionary();

        private void ScanCommandHandlersFrom(IEnjoyTypeScanner scanner)
        {
            var handlers = HandlerHelper.GetHandlersOf(typeof(ICommandHandler<>), scanner);

            foreach (var handler in handlers)
            {
                _enjoyCommandHandlers.Add(new HandlerMetadata(handler.Key, HandlerType.Command), handler.Value);
            }
        }

        private void ScanEventHandlersFrom(IEnjoyTypeScanner scanner)
        {
            var handlers = HandlerHelper.GetHandlersOf(typeof(IEventHandler<>), scanner);

            foreach (var handler in handlers)
            {
                _enjoyEventHandlers.Add(new HandlerMetadata(handler.Key, HandlerType.Event), handler.Value);
            }
        }


        public HandlerScanner(IEnjoyTypeScanner enjoyTypeScanner)
        {
            _enjoyTypeScanner = enjoyTypeScanner;
        }

        public HandlerDictionary Scan()
        {
            ScanCommandHandlersFrom(_enjoyTypeScanner);
            ScanEventHandlersFrom(_enjoyTypeScanner);

            Dictionary<HandlerMetadata, IList<Type>> handlers = _enjoyCommandHandlers.Union(_enjoyEventHandlers).ToDictionary(e => e.Key, h => h.Value);

            var registrationHandlers = new HandlerDictionary(handlers);

            return registrationHandlers;
        }
    }
}