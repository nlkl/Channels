using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IMVar<T> : IChannel<T>
    {
        PotentialValue<T> TryPeek(int millisecondsTimeout);
        PotentialValue<T> TryPeek(int millisecondsTimeout, CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        PotentialValue<T> TryTake(int millisecondsTimeout);
        PotentialValue<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        bool TryPut(T value, int millisecondsTimeout);
        bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
