using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class SynchronousChannel<T> : IChannel<T>
    {
        private readonly SemaphoreSlim hasPutSignal = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim hasTakenSignal = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim readerReadySignal = new SemaphoreSlim(0, 1);
        private readonly MVar<T> valueCell = new MVar<T>();

        public PotentialValue<T> TryPeek() => valueCell.TryPeek();
        public T Peek() => valueCell.Peek();
        public T Peek(CancellationToken cancellationToken) => valueCell.Peek(cancellationToken);
        public Task<T> PeekAsync() => valueCell.PeekAsync();
        public Task<T> PeekAsync(CancellationToken cancellationToken) => valueCell.PeekAsync();

        public PotentialValue<T> TryTake()
        {
            var value = valueCell.TryTake();
            if (value.HasValue)
            {
                hasTakenSignal.Release();
            }
            return value;
        }

        public T Take()
        {
            readerReadySignal.Release();
            var value = valueCell.Take();
            // hasTakenSignal.Release();
            return value;
        }

        public T Take(CancellationToken cancellationToken)
        {
            var value = valueCell.Take(cancellationToken);
            hasTakenSignal.Release();
            return value;
        }

        public async Task<T> TakeAsync()
        {
            var value = await valueCell.TakeAsync().ConfigureAwait(false);
            hasTakenSignal.Release();
            return value;
        }

        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            var value = await valueCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            hasTakenSignal.Release();
            return value;
        }

        public bool TryPut(T value)
        {
            // TODO: Make atomic
            if (valueCell.TryPut(value))
            {
                return hasTakenSignal.Wait(0);
            }
            return false;
        }

        public void Put(T value)
        {
            valueCell.Put(value);
            readerReadySignal.Wait();
            // hasTakenSignal.Wait();
        }

        public void Put(T value, CancellationToken cancellationToken)
        {
            // --TODO: Make atomic
            readerReadySignal.Wait(cancellationToken);
            try
            {
                valueCell.Put(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                readerReadySignal.Release();
            }
            //hasTakenSignal.Wait(cancellationToken);
        }

        public async Task PutAsync(T value)
        {
            await valueCell.PutAsync(value).ConfigureAwait(false);
            await hasTakenSignal.WaitAsync().ConfigureAwait(false);
        }

        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            // TODO: Make atomic
            await valueCell.PutAsync(value, cancellationToken).ConfigureAwait(false);
            await hasTakenSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
