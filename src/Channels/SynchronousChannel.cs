using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class SynchronousChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly AsyncBarrier _barrier = new AsyncBarrier(2);
        private readonly MVar<T> _valueCell = new MVar<T>();

        public PotentialValue<T> TryInspect() => _valueCell.TryInspect();

        public T Inspect() => Inspect(_emptyCancellationToken);
        public T Inspect(CancellationToken cancellationToken) => _valueCell.Inspect(cancellationToken);

        public Task<T> InspectAsync() => InspectAsync(_emptyCancellationToken);
        public Task<T> InspectAsync(CancellationToken cancellationToken) => _valueCell.InspectAsync(cancellationToken);

        public PotentialValue<T> TryRead()
        {
            var result = PotentialValue<T>.WithoutValue();

            if (_readLock.Wait(0))
            {
                if (_barrier.SignalAndWait(0, _emptyCancellationToken))
                {
                    var value = _valueCell.Read();
                    result = PotentialValue<T>.WithValue(value);
                }

                _readLock.Release();
            }

            return result;
        }

        public T Read() => Read(_emptyCancellationToken);
        public T Read(CancellationToken cancellationToken)
        {
            _readLock.Wait(cancellationToken);
            try
            {
                _barrier.SignalAndWait(Timeout.Infinite, cancellationToken);
                return _valueCell.Read();
            }
            finally
            {
                _readLock.Release();
            }
        }

        public Task<T> ReadAsync() => ReadAsync(_emptyCancellationToken);
        public async Task<T> ReadAsync(CancellationToken cancellationToken)
        {
            await _readLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return _valueCell.Read();
            }
            finally
            {
                _readLock.Release();
            }
        }

        public bool TryWrite(T value)
        {
            var success = false;

            if (_writeLock.Wait(0))
            {
                if (_barrier.SignalAndWait(0, _emptyCancellationToken))
                {
                    _valueCell.Write(value);
                    success = true;
                }

                _writeLock.Release();
            }

            return success;
        }

        public void Write(T value) => Write(value, _emptyCancellationToken);
        public void Write(T value, CancellationToken cancellationToken)
        {
            _writeLock.Wait(cancellationToken);
            try
            {
                _barrier.SignalAndWait(Timeout.Infinite, cancellationToken);
                _valueCell.Write(value);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public Task WriteAsync(T value) => WriteAsync(value, _emptyCancellationToken);
        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _barrier.SignalAndWaitAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                _valueCell.Write(value);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken)
        {
            // TODO: Find a good way to implement selection on sync channels
            throw new NotImplementedException();
        }
    }
}
