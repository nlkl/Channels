using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IMVar<T>
    {
        T Read();
        bool TryRead(out T value);
        bool TryRead(out T value, int millisecondsTimeout);
        bool TryRead(out T value, CancellationToken cancellationToken);
        bool TryRead(out T value, int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> ReadAsync();
        Task<Result<T>> TryReadAsync();
        Task<Result<T>> TryReadAsync(int millisecondsTimeout);
        Task<Result<T>> TryReadAsync(CancellationToken cancellationToken);
        Task<Result<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        T Take();
        bool TryTake(out T value);
        bool TryTake(out T value, int millisecondsTimeout);
        bool TryTake(out T value, CancellationToken cancellationToken);
        bool TryTake(out T value, int millisecondsTimeout, CancellationToken cancellationToken);
        Task<T> TakeAsync();

        void Put(T value);
        bool TryPut(T value);
        bool TryPut(T value, int millisecondsTimeout);
        bool TryPut(T value, CancellationToken cancellationToken);
        bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken);
        Task PutAsync(T value);
    }
}
