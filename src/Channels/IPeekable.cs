using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IPeekable<T>
    {
        PotentialValue<T> TryPeek();
        T Peek();
        T Peek(CancellationToken cancellationToken);
        Task<T> PeekAsync();
        Task<T> PeekAsync(CancellationToken cancellationToken);
    }
}
