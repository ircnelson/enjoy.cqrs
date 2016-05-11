using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace EnjoyCQRS.IntegrationTests.Extensions
{
    public static class CustomRegistrationExtensions
    {
        // This is the important custom bit: Registering a named service during scanning.
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsNamedClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType,
            Func<Type, object> keyFactory)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null) throw new ArgumentNullException("openGenericServiceType");

            return registration
                .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
                .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType).Select(t => (Service)new KeyedService(keyFactory(t), t)));
        }

        // These next two methods are basically copy/paste of some Autofac internals that
        // are used to determine closed generic types during scanning.
        public static IEnumerable<Type> GetTypesThatClose(this Type candidateType, Type openGenericServiceType)
        {
            return candidateType.GetInterfaces().Concat(TraverseAcross(candidateType, t => t.BaseType)).Where(t => t.IsClosedTypeOf(openGenericServiceType));
        }

        public static IEnumerable<T> TraverseAcross<T>(T first, Func<T, T> next) where T : class
        {
            var item = first;
            while (item != null)
            {
                yield return item;
                item = next(item);
            }
        }
    }
}