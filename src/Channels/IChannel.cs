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

        PotentialValue<T> TryReceive();
        T Receive();
        T Receive(CancellationToken cancellationToken);
        Task<T> ReceiveAsync();
        Task<T> ReceiveAsync(CancellationToken cancellationToken);

        bool TrySend(T value);
        void Send(T value);
        void Send(T value, CancellationToken cancellationToken);
        Task SendAsync(T value);
        Task SendAsync(T value, CancellationToken cancellationToken);
    }
}
