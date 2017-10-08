using System.Threading;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections
{
    public interface IProjectionRebuilder
    {
        Task RebuildAsync<T>(T eventStreamReader, CancellationToken cancellationToken = default(CancellationToken)) where T : EventStreamReader;
    }
}