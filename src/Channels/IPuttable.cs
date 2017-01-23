using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IPuttable<T>
    {
        bool TryPut(T value);
        void Put(T value);
        void Put(T value, CancellationToken cancellationToken);
        Task PutAsync(T value);
        Task PutAsync(T value, CancellationToken cancellationToken);
    }
}
