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

        public T Read() => TryRead(Timeout.Infinite, emptyCancellationToken).Value;
        public Result<T> TryRead() => TryRead(0, emptyCancellationToken);
        public Result<T> TryRead(int millisecondsTimeout) => TryRead(millisecondsTimeout, emptyCancellationToken);
        public Result<T> TryRead(CancellationToken cancellationToken) => TryRead(Timeout.Infinite, cancellationToken);

        public Result<T> TryRead(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = this.value;
                canTakeSignal.Release();
                return Result<T>.Ok(value);
            }

            return Result<T>.Error();
        }

        public async Task<T> ReadAsync() => (await TryReadAsync(Timeout.Infinite, emptyCancellationToken).ConfigureAwait(false)).Value;
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

        public T Take() => TryTake(Timeout.Infinite, emptyCancellationToken).Value;
        public Result<T> TryTake() => TryTake(0, emptyCancellationToken);
        public Result<T> TryTake(int millisecondsTimeout) => TryTake(millisecondsTimeout, emptyCancellationToken);
        public Result<T> TryTake(CancellationToken cancellationToken) => TryTake(Timeout.Infinite, cancellationToken);

        public Result<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = this.value;
                this.value = default(T);
                canPutSignal.Release();
                return Result<T>.Ok(value);
            }

            return Result<T>.Error();
        }

        public async Task<T> TakeAsync() => (await TryTakeAsync(Timeout.Infinite, emptyCancellationToken).ConfigureAwait(false)).Value;
        public Task<Result<T>> TryTakeAsync() => TryTakeAsync(0, emptyCancellationToken);
        public Task<Result<T>> TryTakeAsync(int millisecondsTimeout) => TryTakeAsync(millisecondsTimeout, emptyCancellationToken);
        public Task<Result<T>> TryTakeAsync(CancellationToken cancellationToken) => TryTakeAsync(Timeout.Infinite, cancellationToken);

        public async Task<Result<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = this.value;
                this.value = default(T);
                canPutSignal.Release();
                return Result<T>.Ok(value);
            }

            return Result<T>.Error();
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

        public async Task<bool> TryPutAsync(T value) => await TryPutAsync(value, 0, emptyCancellationToken).ConfigureAwait(false);
        public Task<bool> TryPutAsync(T value, int millisecondsTimeout) => TryPutAsync(value, millisecondsTimeout, emptyCancellationToken);
        public Task<bool> TryPutAsync(T value, CancellationToken cancellationToken) => TryPutAsync(value, Timeout.Infinite, cancellationToken);

        public async Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await canPutSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                this.value = value;
                canTakeSignal.Release();
                return true;
            }

            return false;
        }
    }
}
