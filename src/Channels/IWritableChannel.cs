using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IWritableChannel<T>
    {
        bool TryWrite(T value);
        void Write(T value);
        void Write(T value, CancellationToken cancellationToken);
        Task WriteAsync(T value);
        Task WriteAsync(T value, CancellationToken cancellationToken);
    }
}
