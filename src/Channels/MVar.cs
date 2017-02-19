using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class MVar<T> : IMVar<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim _canWriteSignal = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _canReadSignal = new SemaphoreSlim(0, 1);

        private T _value;

        public MVar()
        {
            _canWriteSignal.Release();
        }

        public MVar(T value)
        {
            _value = value;
            _canReadSignal.Release();
        }

        public T Inspect() => TryInspect(Timeout.Infinite, _emptyCancellationToken).Value;
        public T Inspect(CancellationToken cancellationToken) => TryInspect(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryInspect() => TryInspect(0, _emptyCancellationToken);
        public PotentialValue<T> TryInspect(int millisecondsTimeout) => TryInspect(millisecondsTimeout, _emptyCancellationToken);

        public PotentialValue<T> TryInspect(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canReadSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = _value;
                _canReadSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> InspectAsync() => (await TryInspectAsync(Timeout.Infinite, _emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> InspectAsync(CancellationToken cancellationToken) => (await TryInspectAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryInspectAsync(int millisecondsTimeout) => TryInspectAsync(millisecondsTimeout, _emptyCancellationToken);

        public async Task<PotentialValue<T>> TryInspectAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canReadSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = _value;
                _canReadSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public T Read() => TryRead(Timeout.Infinite, _emptyCancellationToken).Value;
        public T Read(CancellationToken cancellationToken) => TryRead(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryRead() => TryRead(0, _emptyCancellationToken);
        public PotentialValue<T> TryRead(int millisecondsTimeout) => TryRead(millisecondsTimeout, _emptyCancellationToken);

        public PotentialValue<T> TryRead(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canReadSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = _value;
                _value = default(T);
                _canWriteSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> ReadAsync() => (await TryReadAsync(Timeout.Infinite, _emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> ReadAsync(CancellationToken cancellationToken) => (await TryReadAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout) => TryReadAsync(millisecondsTimeout, _emptyCancellationToken);

        public async Task<PotentialValue<T>> TryReadAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canReadSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = _value;
                _value = default(T);
                _canWriteSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public void Write(T value) => TryWrite(value, Timeout.Infinite, _emptyCancellationToken);
        public void Write(T value, CancellationToken cancellationToken) => TryWrite(value, Timeout.Infinite, cancellationToken);
        public bool TryWrite(T value) => TryWrite(value, 0, _emptyCancellationToken);
        public bool TryWrite(T value, int millisecondsTimeout) => TryWrite(value, millisecondsTimeout, _emptyCancellationToken);

        public bool TryWrite(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canWriteSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                _value = value;
                _canReadSignal.Release();
                return true;
            }

            return false;
        }

        public Task WriteAsync(T value) => TryWriteAsync(value, Timeout.Infinite, _emptyCancellationToken);
        public Task WriteAsync(T value, CancellationToken cancellationToken) => TryWriteAsync(value, Timeout.Infinite, cancellationToken);
        public Task<bool> TryWriteAsync(T value, int millisecondsTimeout) => TryWriteAsync(value, millisecondsTimeout, _emptyCancellationToken);

        public async Task<bool> TryWriteAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canWriteSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                _value = value;
                _canReadSignal.Release();
                return true;
            }

            return false;
        }

        public async Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken)
        {
            await _canReadSignal.WaitAsync(cancellationToken).ConfigureAwait(false);

            return new Selectable<T>(async waitUntilSelected =>
            {
                try
                {
                    await waitUntilSelected(cancellationToken).ConfigureAwait(false);
                    var value = _value;
                    _value = default(T);
                    _canWriteSignal.Release();
                    return value;
                }
                catch
                {
                    _canReadSignal.Release();
                    throw;
                }
            });
        }
    }
}
