using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class MVar<T> : IMVar<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly SemaphoreSlim _canPutSignal = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _canTakeSignal = new SemaphoreSlim(0, 1);

        private T _value;

        public MVar()
        {
            _canPutSignal.Release();
        }

        public MVar(T value)
        {
            _value = value;
            _canTakeSignal.Release();
        }

        public T Peek() => TryPeek(Timeout.Infinite, _emptyCancellationToken).Value;
        public T Peek(CancellationToken cancellationToken) => TryPeek(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryPeek() => TryPeek(0, _emptyCancellationToken);
        public PotentialValue<T> TryPeek(int millisecondsTimeout) => TryPeek(millisecondsTimeout, _emptyCancellationToken);

        public PotentialValue<T> TryPeek(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = _value;
                _canTakeSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> PeekAsync() => (await TryPeekAsync(Timeout.Infinite, _emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> PeekAsync(CancellationToken cancellationToken) => (await TryPeekAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout) => TryPeekAsync(millisecondsTimeout, _emptyCancellationToken);

        public async Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = _value;
                _canTakeSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public T Take() => TryTake(Timeout.Infinite, _emptyCancellationToken).Value;
        public T Take(CancellationToken cancellationToken) => TryTake(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryTake() => TryTake(0, _emptyCancellationToken);
        public PotentialValue<T> TryTake(int millisecondsTimeout) => TryTake(millisecondsTimeout, _emptyCancellationToken);

        public PotentialValue<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = _value;
                _value = default(T);
                _canPutSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> TakeAsync() => (await TryTakeAsync(Timeout.Infinite, _emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> TakeAsync(CancellationToken cancellationToken) => (await TryTakeAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout) => TryTakeAsync(millisecondsTimeout, _emptyCancellationToken);

        public async Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = _value;
                _value = default(T);
                _canPutSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public void Put(T value) => TryPut(value, Timeout.Infinite, _emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken) => TryPut(value, Timeout.Infinite, cancellationToken);
        public bool TryPut(T value) => TryPut(value, 0, _emptyCancellationToken);
        public bool TryPut(T value, int millisecondsTimeout) => TryPut(value, millisecondsTimeout, _emptyCancellationToken);

        public bool TryPut(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (_canPutSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                _value = value;
                _canTakeSignal.Release();
                return true;
            }

            return false;
        }

        public Task PutAsync(T value) => TryPutAsync(value, Timeout.Infinite, _emptyCancellationToken);
        public Task PutAsync(T value, CancellationToken cancellationToken) => TryPutAsync(value, Timeout.Infinite, cancellationToken);
        public Task<bool> TryPutAsync(T value, int millisecondsTimeout) => TryPutAsync(value, millisecondsTimeout, _emptyCancellationToken);

        public async Task<bool> TryPutAsync(T value, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await _canPutSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                _value = value;
                _canTakeSignal.Release();
                return true;
            }

            return false;
        }
    }
}
