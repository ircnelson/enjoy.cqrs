using EnjoyCQRS.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EnjoyCQRS.EventSource.Projections
{
    public class ProjectionProviderAttributeScanner : IProjectionProviderScanner
    {
        private static readonly ConcurrentDictionary<string, HashSet<IProjectionProvider>> _cachedProviders = new ConcurrentDictionary<string, HashSet<IProjectionProvider>>();

        public Task<ScannerResult> ScanAsync(Type type)
        {
            if (!typeof(IAggregate).IsAssignableFrom(type)) throw new TargetException($"The target should be {nameof(IAggregate)}.");

            var providers = _cachedProviders.GetOrAdd(type.FullName, (key) =>
            {
                var _providers = type
                    .GetTypeInfo()
                    .GetCustomAttributes<ProjectionProviderAttribute>()
                    .Select(e => e.Provider)
                    .Distinct()
                    .Select(e => Activator.CreateInstance(e))
                    .Cast<IProjectionProvider>();

                return new HashSet<IProjectionProvider>(_providers);
            });

            var result = new ScannerResult(providers);

            return Task.FromResult(result);
        }

        public async Task<ScannerResult> ScanAsync<TAggregate>() where TAggregate : IAggregate
        {
            return await ScanAsync(typeof(TAggregate));
        }
    }

    public class ScannerResult
    {
        public IEnumerable<IProjectionProvider> Providers { get; private set; }

        public ScannerResult(IEnumerable<IProjectionProvider> providers)
        {
            Providers = providers;
        }
    }
}
