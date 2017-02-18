using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IChannel<T> : IWritableChannel<T>, IReadableChannel<T>, IInspectableChannel<T>, ISelectableChannel<T>
    {
    }
}
