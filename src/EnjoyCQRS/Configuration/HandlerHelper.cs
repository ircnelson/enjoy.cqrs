using System;
using System.Collections.Generic;
using System.Linq;

namespace EnjoyCQRS.Configuration
{
    internal class HandlerHelper
    {
        public static IDictionary<Type, IList<Type>> GetHandlersOf(Type handlerType, IEnjoyTypeScanner scanner)
        {
            var commands = new Dictionary<Type, IList<Type>>();

            scanner.Scan()
                .Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == handlerType))
                .ToList()
                .ForEach(x => AddItem(handlerType, commands, x));
            return commands;
        }

        public static IEnumerable<Type> Get<TEventOrCommand>(IEnjoyTypeScanner scanner)
        {
            return scanner.Scan()
                .Where(x => typeof(TEventOrCommand).IsAssignableFrom(x) || x.BaseType == typeof(TEventOrCommand))
                .ToList();
        }

        private static void AddItem(Type handlerType, IDictionary<Type, IList<Type>> dictionary, Type type)
        {
            var dto = type.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType)
                .First()
                .GetGenericArguments()
                .First();

            if (!dictionary.ContainsKey(dto))
                dictionary.Add(dto, new List<Type>());

            dictionary[dto].Add(type);
        }
    }
}