using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class SynchronousChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim _takeLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _putLock = new SemaphoreSlim(1, 1);
        private readonly AsyncBarrier _barrier = new AsyncBarrier(2);
        private readonly MVar<T> _valueCell = new MVar<T>();

        public PotentialValue<T> TryPeek() => _valueCell.TryPeek();

        public T Peek() => Peek(_emptyCancellationToken);
        public T Peek(CancellationToken cancellationToken) => _valueCell.Peek(cancellationToken);

        public Task<T> PeekAsync() => PeekAsync(_emptyCancellationToken);
        public Task<T> PeekAsync(CancellationToken cancellationToken) => _valueCell.PeekAsync(cancellationToken);

        public PotentialValue<T> TryTake()
        {
            var result = PotentialValue<T>.WithoutValue();

            if (_takeLock.Wait(0))
            {
                if (_barrier.SignalAndWait(0, _emptyCancellationToken))
                {
                    var value = _valueCell.Take();
                    result = PotentialValue<T>.WithValue(value);
                }

                _takeLock.Release();
            }

            return result;
        }

        public T Take() => Take(_emptyCancellationToken);
        public T Take(CancellationToken cancellationToken)
        {
            _takeLock.Wait(cancellationToken);
            try
            {
                _barrier.SignalAndWait(Timeout.Infinite, cancellationToken);
                return _valueCell.Take();
            }
            finally
            {
                _takeLock.Release();
            }
        }

        public Task<T> TakeAsync() => TakeAsync(_emptyCancellationToken);
        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            await _takeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return _valueCell.Take();
            }
            finally
            {
                _takeLock.Release();
            }
        }

        public bool TryPut(T value)
        {
            var success = false;

            if (_putLock.Wait(0))
            {
                if (_barrier.SignalAndWait(0, _emptyCancellationToken))
                {
                    _valueCell.Put(value);
                    success = true;
                }

                _putLock.Release();
            }

            return success;
        }

        public void Put(T value) => Put(value, _emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken)
        {
            _putLock.Wait(cancellationToken);
            try
            {
                _barrier.SignalAndWait(Timeout.Infinite, cancellationToken);
                _valueCell.Put(value);
            }
            finally
            {
                _putLock.Release();
            }
        }

        public Task PutAsync(T value) => PutAsync(value, _emptyCancellationToken);
        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            await _putLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                _valueCell.Put(value);
            }
            finally
            {
                _putLock.Release();
            }
        }
    }
}
