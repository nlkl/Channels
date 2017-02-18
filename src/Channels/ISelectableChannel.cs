using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface ISelectableChannel<T>
    {
        Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken);
    }
}
