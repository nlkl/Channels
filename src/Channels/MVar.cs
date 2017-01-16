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

        public T Peek() => TryPeek(Timeout.Infinite, emptyCancellationToken).Value;
        public T Peek(CancellationToken cancellationToken) => TryPeek(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryPeek() => TryPeek(0, emptyCancellationToken);
        public PotentialValue<T> TryPeek(int millisecondsTimeout) => TryPeek(millisecondsTimeout, emptyCancellationToken);

        public PotentialValue<T> TryPeek(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = this.value;
                canTakeSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> PeekAsync() => (await TryPeekAsync(Timeout.Infinite, emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> PeekAsync(CancellationToken cancellationToken) => (await TryPeekAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout) => TryPeekAsync(millisecondsTimeout, emptyCancellationToken);

        public async Task<PotentialValue<T>> TryPeekAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = this.value;
                canTakeSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public T Take() => TryTake(Timeout.Infinite, emptyCancellationToken).Value;
        public T Take(CancellationToken cancellationToken) => TryTake(Timeout.Infinite, cancellationToken).Value;
        public PotentialValue<T> TryTake() => TryTake(0, emptyCancellationToken);
        public PotentialValue<T> TryTake(int millisecondsTimeout) => TryTake(millisecondsTimeout, emptyCancellationToken);

        public PotentialValue<T> TryTake(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (canTakeSignal.Wait(millisecondsTimeout, cancellationToken))
            {
                var value = this.value;
                this.value = default(T);
                canPutSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public async Task<T> TakeAsync() => (await TryTakeAsync(Timeout.Infinite, emptyCancellationToken).ConfigureAwait(false)).Value;
        public async Task<T> TakeAsync(CancellationToken cancellationToken) => (await TryTakeAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false)).Value;
        public Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout) => TryTakeAsync(millisecondsTimeout, emptyCancellationToken);

        public async Task<PotentialValue<T>> TryTakeAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var hasSignal = await canTakeSignal
                .WaitAsync(millisecondsTimeout, cancellationToken)
                .ConfigureAwait(false);

            if (hasSignal)
            {
                var value = this.value;
                this.value = default(T);
                canPutSignal.Release();
                return PotentialValue<T>.WithValue(value);
            }

            return PotentialValue<T>.WithoutValue();
        }

        public void Put(T value) => TryPut(value, Timeout.Infinite, emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken) => TryPut(value, Timeout.Infinite, cancellationToken);
        public bool TryPut(T value) => TryPut(value, 0, emptyCancellationToken);
        public bool TryPut(T value, int millisecondsTimeout) => TryPut(value, millisecondsTimeout, emptyCancellationToken);

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

        public Task PutAsync(T value) => TryPutAsync(value, Timeout.Infinite, emptyCancellationToken);
        public Task PutAsync(T value, CancellationToken cancellationToken) => TryPutAsync(value, Timeout.Infinite, cancellationToken);
        public Task<bool> TryPutAsync(T value, int millisecondsTimeout) => TryPutAsync(value, millisecondsTimeout, emptyCancellationToken);

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
