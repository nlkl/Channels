using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class SynchronousChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim takeLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim putLock = new SemaphoreSlim(1, 1);
        private readonly AsyncBarrier barrier = new AsyncBarrier(2);
        private readonly MVar<T> valueCell = new MVar<T>();

        public PotentialValue<T> TryPeek() => valueCell.TryPeek();
        public T Peek() => valueCell.Peek();
        public T Peek(CancellationToken cancellationToken) => valueCell.Peek(cancellationToken);
        public Task<T> PeekAsync() => valueCell.PeekAsync();
        public Task<T> PeekAsync(CancellationToken cancellationToken) => valueCell.PeekAsync();

        public PotentialValue<T> TryTake() => TryTake(false, emptyCancellationToken);
        public T Take() => TryTake(true, emptyCancellationToken).Value;
        public T Take(CancellationToken cancellationToken) => TryTake(true, cancellationToken).Value;

        private PotentialValue<T> TryTake(bool wait, CancellationToken cancellationToken)
        {
            var timeout = wait ? Timeout.Infinite : 0;
            if (!takeLock.Wait(timeout, cancellationToken)) return PotentialValue<T>.WithoutValue();
            try
            {
                if (barrier.SignalAndWait(timeout, cancellationToken))
                {
                    var value = valueCell.Take();
                    return PotentialValue<T>.WithValue(value);
                }

                return PotentialValue<T>.WithoutValue();
            }
            finally
            {
                takeLock.Release();
            }
        }

        public Task<T> TakeAsync() => TakeAsync(emptyCancellationToken);

        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            await takeLock.WaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            try
            {
                await barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return valueCell.Take();
            }
            finally
            {
                takeLock.Release();
            }
        }

        public bool TryPut(T value) => TryPut(value, false, emptyCancellationToken);
        public void Put(T value) => TryPut(value, true, emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken) => TryPut(value, true, cancellationToken);

        private bool TryPut(T value, bool wait, CancellationToken cancellationToken)
        {
            var timeout = wait ? Timeout.Infinite : 0;
            if (!putLock.Wait(timeout, cancellationToken)) return false;
            try
            {
                if (barrier.SignalAndWait(timeout, cancellationToken))
                {
                    valueCell.Put(value);
                    return true;
                }

                return false;
            }
            finally
            {
                putLock.Release();
            }
        }

        public Task PutAsync(T value) => PutAsync(value, emptyCancellationToken);

        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            await putLock.WaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            try
            {
                await barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                valueCell.Put(value);
            }
            finally
            {
                putLock.Release();
            }
        }
    }
}
