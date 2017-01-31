using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IInspectableChannel<T>
    {
        PotentialValue<T> TryInspect();
        T Inspect();
        T Inspect(CancellationToken cancellationToken);
        Task<T> InspectAsync();
        Task<T> InspectAsync(CancellationToken cancellationToken);
    }
}
