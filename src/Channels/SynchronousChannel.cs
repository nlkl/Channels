using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class SynchronousChannel<T> : IChannel<T>
    {
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
                readerReadySignal.Release();
            }
            return value;
        }

        public T Take()
        {
            readerReadySignal.Release();
            var value = valueCell.Take();
            return value;
        }

        public T Take(CancellationToken cancellationToken)
        {
            var value = valueCell.Take(cancellationToken);
            readerReadySignal.Release();
            return value;
        }

        public async Task<T> TakeAsync()
        {
            readerReadySignal.Release();
            var value = await valueCell.TakeAsync().ConfigureAwait(false);
            return value;
        }

        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            var value = await valueCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            readerReadySignal.Release();
            return value;
        }

        public bool TryPut(T value)
        {
            if (!readerReadySignal.Wait(0)) return false;
            if (valueCell.TryPut(value)) return true;
            readerReadySignal.Release();
            return false;
        }

        public void Put(T value)
        {
            valueCell.Put(value);
            readerReadySignal.Wait();
        }

        public void Put(T value, CancellationToken cancellationToken)
        {
            readerReadySignal.Wait(cancellationToken);
            try
            {
                valueCell.Put(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                readerReadySignal.Release();
            }
        }

        public async Task PutAsync(T value)
        {
            await valueCell.PutAsync(value).ConfigureAwait(false);
            await readerReadySignal.WaitAsync().ConfigureAwait(false);
        }

        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            await readerReadySignal.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await valueCell.PutAsync(value, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                readerReadySignal.Release();
            }
        }
    }
}
