using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IChannel<T>
    {
        PotentialValue<T> TryPeek();
        T Peek();
        T Peek(CancellationToken cancellationToken);
        Task<T> PeekAsync();
        Task<T> PeekAsync(CancellationToken cancellationToken);

        PotentialValue<T> TryTake();
        T Take();
        T Take(CancellationToken cancellationToken);
        Task<T> TakeAsync();
        Task<T> TakeAsync(CancellationToken cancellationToken);

        bool TryPut(T value);
        void Put(T value);
        void Put(T value, CancellationToken cancellationToken);
        Task PutAsync(T value);
        Task PutAsync(T value, CancellationToken cancellationToken);
    }
}
