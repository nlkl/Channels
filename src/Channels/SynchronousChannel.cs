using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    // NOTE: This is a work-in-progress, and neither correct nor thread-safe yet.
    public class SynchronousChannel<T> : IChannel<T>
    {
        private readonly MVar<object> isPuttingSignal;
        private readonly MVar<object> isTakingSignal;
        private readonly MVar<T> valueCell;

        public PotentialValue<T> TryPeek() => valueCell.TryPeek();
        public T Peek() => valueCell.Peek();
        public T Peek(CancellationToken cancellationToken) => valueCell.Peek(cancellationToken);
        public Task<T> PeekAsync() => valueCell.PeekAsync();
        public Task<T> PeekAsync(CancellationToken cancellationToken) => valueCell.PeekAsync();

        public PotentialValue<T> TryTake()
        {
            var value = PotentialValue<T>.WithoutValue();

            if (!isTakingSignal.TryPut(null)) return value;

            if (isPuttingSignal.TryPeek().HasValue)
            {
                value = valueCell.TryTake();
            }

            isTakingSignal.Take();

            return value;
        }

        public T Take()
        {
            isTakingSignal.Put(null);
            isPuttingSignal.Peek();
            var value = valueCell.Take();
            isTakingSignal.Take();
            return value;
        }

        public T Take(CancellationToken cancellationToken)
        {
            isTakingSignal.Put(null, cancellationToken);
            try
            {
                isPuttingSignal.Peek(cancellationToken);
                return valueCell.Take(cancellationToken);
            }
            finally
            {
                isTakingSignal.Take();
            }
        }

        public async Task<T> TakeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryPut(T value)
        {
            var success = false;

            if (!isPuttingSignal.TryPut(null)) return success;

            if (isTakingSignal.TryPeek().HasValue)
            {
                success = valueCell.TryPut(value);
            }

            isPuttingSignal.Take();
            return success;
        }

        public void Put(T value)
        {
            isPuttingSignal.Put(null);
            isTakingSignal.Peek();
            valueCell.Put(value);
            isPuttingSignal.Take();
        }

        public void Put(T value, CancellationToken cancellationToken)
        {
            isPuttingSignal.Put(null, cancellationToken);
            try
            {
                isTakingSignal.Peek(cancellationToken);
                valueCell.Put(value, cancellationToken);
            }
            finally
            {
                isPuttingSignal.Take();
            }
        }

        public async Task PutAsync(T value)
        {
            throw new NotImplementedException();
        }

        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
