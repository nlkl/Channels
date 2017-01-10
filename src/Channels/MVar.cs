using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class MVar<T> : IMVar<T>
    {
        private static readonly CancellationToken emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim canPutSignal = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim canTakeSignal = new SemaphoreSlim(0, 1);

        private T value;

        public MVar()
        {
            canPutSignal.Release();
        }

        public MVar(T value)
        {
            canTakeSignal.Release();
        }

        public T Read()
        {
            T value;
            TryRead(out value, Timeout.Infinite, emptyCancellationToken);
            return value;
        }

        public bool TryRead(out T value) => TryRead(out value, 0, emptyCancellationToken);
        public bool TryRead(out T value, int millisecondsTimeout) => TryRead(out value, millisecondsTimeout, emptyCancellationToken);
        public bool TryRead(out T value, CancellationToken cancellationToken) => TryRead(out value, Timeout.Infinite, cancellationToken);

        public bool TryRead(out T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                value = this.value;
                canTakeSignal.Release();
                return true;
            }

            value = default(T);
            return false;
        }

        public async Task<T> ReadAsync()
        {
            await canTakeSignal.WaitAsync().ConfigureAwait(false);
            var value = this.value;
            canTakeSignal.Release();
            return value;
        }

        public Task<Result<T>> TryReadAsync() => TryReadAsync(0, emptyCancellationToken);
        public Task<Result<T>> TryReadAsync(int millisecondsTimeout) => TryReadAsync(millisecondsTimeout, emptyCancellationToken);
        public Task<Result<T>> TryReadAsync(CancellationToken cancellationToken) => TryReadAsync(Timeout.Infinite, cancellationToken);

        public async Task<Result<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = this.value;
                canTakeSignal.Release();
                return Result<T>.Ok(value);
            }

            return Result<T>.Error();
        }

        public T Take()
        {
            T value;
            TryTake(out value, Timeout.Infinite, emptyCancellationToken);
            return value;
        }

        public bool TryTake(out T value) => TryTake(out value, 0, emptyCancellationToken);
        public bool TryTake(out T value, int millisecondsTimeout) => TryTake(out value, millisecondsTimeout, emptyCancellationToken);
        public bool TryTake(out T value, CancellationToken cancellationToken) => TryTake(out value, Timeout.Infinite, cancellationToken);

        public bool TryTake(out T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                value = this.value;
                this.value = default(T);
                canPutSignal.Release();
                return true;
            }

            value = default(T);
            return false;
        }

        public async Task<T> TakeAsync()
        {
            await canTakeSignal.WaitAsync().ConfigureAwait(false);
            var value = this.value;
            this.value = default(T);
            canPutSignal.Release();
            return value;
        }

        public void Put(T value) => TryPut(value, Timeout.Infinite, emptyCancellationToken);

        public bool TryPut(T value) => TryPut(value, 0, emptyCancellationToken);
        public bool TryPut(T value, int millisecondsTimeout) => TryPut(value, millisecondsTimeout, emptyCancellationToken);
        public bool TryPut(T value, CancellationToken cancellationToken) => TryPut(value, Timeout.Infinite, cancellationToken);

        public bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canPutSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                this.value = value;
                canTakeSignal.Release();
                return true;
            }

            return false;
        }

        public async Task PutAsync(T value)
        {
            await canPutSignal.WaitAsync().ConfigureAwait(false);
            this.value = value;
            canTakeSignal.Release();
        }
    }
}
