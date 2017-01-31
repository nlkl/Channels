using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IReadableChannel<T>
    {
        PotentialValue<T> TryRead();
        T Read();
        T Read(CancellationToken cancellationToken);
        Task<T> ReadAsync();
        Task<T> ReadAsync(CancellationToken cancellationToken);
    }
}
