using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface ITakeable<T>
    {
        PotentialValue<T> TryTake();
        T Take();
        T Take(CancellationToken cancellationToken);
        Task<T> TakeAsync();
        Task<T> TakeAsync(CancellationToken cancellationToken);
    }
}
