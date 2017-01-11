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
        Result<T> TryRead();
        Result<T> TryRead(int millisecondsTimeout);
        Result<T> TryRead(CancellationToken cancellationToken);
        Result<T> TryRead(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> ReadAsync();
        Task<Result<T>> TryReadAsync();
        Task<Result<T>> TryReadAsync(int millisecondsTimeout);
        Task<Result<T>> TryReadAsync(CancellationToken cancellationToken);
        Task<Result<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        T Take();
        Result<T> TryTake();
        Result<T> TryTake(int millisecondsTimeout);
        Result<T> TryTake(CancellationToken cancellationToken);
        Result<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> TakeAsync();
        Task<Result<T>> TryTakeAsync();
        Task<Result<T>> TryTakeAsync(int millisecondsTimeout);
        Task<Result<T>> TryTakeAsync(CancellationToken cancellationToken);
        Task<Result<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        void Put(T value);
        bool TryPut(T value);
        bool TryPut(T value, int millisecondsTimeout);
        bool TryPut(T value, CancellationToken cancellationToken);
        bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken);

        Task PutAsync(T value);
        Task<bool> TryPutAsync(T value);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout);
        Task<bool> TryPutAsync(T value, CancellationToken cancellationToken);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
