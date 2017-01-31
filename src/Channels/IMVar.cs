using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IMVar<T> : IChannel<T>
    {
        PotentialValue<T> TryInspect(int millisecondsTimeout);
        PotentialValue<T> TryInspect(int millisecondsTimeout, CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryInspectAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryInspectAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        PotentialValue<T> TryRead(int millisecondsTimeout);
        PotentialValue<T> TryRead(int millisecondsTimeout, CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        bool TryWrite(T value, int millisecondsTimeout);
        bool TryWrite(T value, int millisecondsTimeout, CancellationToken cancellationToken);
        Task<bool> TryWriteAsync(T value, int millisecondsTimeout);
        Task<bool> TryWriteAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
