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
        T Read(CancellationToken cancellationToken);
        PotentialValue<T> TryRead();
        PotentialValue<T> TryRead(int millisecondsTimeout);
        PotentialValue<T> TryRead(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> ReadAsync();
        Task<T> ReadAsync(CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        T Take();
        T Take(CancellationToken cancellationToken);
        PotentialValue<T> TryTake();
        PotentialValue<T> TryTake(int millisecondsTimeout);
        PotentialValue<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<T> TakeAsync();
        Task<T> TakeAsync(CancellationToken cancellationToken);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout);
        Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        void Put(T value);
        void Put(T value, CancellationToken cancellationToken);
        bool TryPut(T value);
        bool TryPut(T value, int millisecondsTimeout);
        bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken);

        Task PutAsync(T value);
        Task PutAsync(T value, CancellationToken cancellationToken);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout);
        Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
