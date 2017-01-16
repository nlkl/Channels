using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IMVar<T>
    {
        T Peek();
        T Peek(CancellationToken cancellationToken);
        PotentialValue<T> TryPeek();
        PotentialValue<T> TryPeek(int millisecondsTimeout);
        PotentialValue<T> TryPeek(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> PeekAsync();
        Task<T> PeekAsync(CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        T Take();
        T Take(CancellationToken cancellationToken);
        PotentialValue<T> TryTake();
        PotentialValue<T> TryTake(int millisecondsTimeout);
        PotentialValue<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> TakeAsync();
        Task<T> TakeAsync(CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        void Put(T value);
        void Put(T value, CancellationToken cancellationToken);
        bool TryPut(T value);
        bool TryPut(T value, int millisecondsTimeout);
        bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken);

        Task PutAsync(T value);
        Task PutAsync(T value, CancellationToken cancellationToken);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
