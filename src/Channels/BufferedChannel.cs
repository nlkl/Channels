using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class BufferedChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly int _capacity;
        private readonly MVar<int> _incomingIndexCell;
        private readonly MVar<int> _outgoingIndexCell;
        private readonly MVar<T>[] _buffer;

        public BufferedChannel(int capacity)
        {
            if (capacity < 1) throw new ArgumentException(nameof(capacity), "Buffer must have a capacity greater than zero.");

            _capacity = capacity;
            _incomingIndexCell = new MVar<int>(0);
            _outgoingIndexCell = new MVar<int>(0);

            _buffer = new MVar<T>[capacity];
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = new MVar<T>();
            }
        }

        public PotentialValue<T> TryPeek()
        {
            var result = PotentialValue<T>.WithoutValue();

            int index;
            if (_outgoingIndexCell.TryTake().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                T value;
                if (valueCell.TryPeek().TryGetValue(out value))
                {
                    result = PotentialValue<T>.WithValue(value);
                }

                _outgoingIndexCell.Put(index);
            }

            return result;
        }

        public T Peek() => Peek(_emptyCancellationToken);
        public T Peek(CancellationToken cancellationToken)
        {
            var index = _outgoingIndexCell.Take(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                var value = valueCell.Peek(cancellationToken);
                return value;
            }
            finally
            {
                _outgoingIndexCell.Put(index);
            }
        }

        public Task<T> PeekAsync() => PeekAsync(_emptyCancellationToken);
        public async Task<T> PeekAsync(CancellationToken cancellationToken)
        {
            var index = await _outgoingIndexCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                var value = await valueCell.PeekAsync(cancellationToken).ConfigureAwait(false);
                return value;
            }
            finally
            {
                _outgoingIndexCell.Put(index);
            }
        }

        public PotentialValue<T> TryTake()
        {
            var result = PotentialValue<T>.WithoutValue();

            int index;
            if (_outgoingIndexCell.TryTake().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                T value;
                if (valueCell.TryTake().TryGetValue(out value))
                {
                    index = NextIndex(index);
                    result = PotentialValue<T>.WithValue(value);
                }

                _outgoingIndexCell.Put(index);
            }

            return result;
        }

        public T Take() => Take(_emptyCancellationToken);
        public T Take(CancellationToken cancellationToken)
        {
            var index = _outgoingIndexCell.Take(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                var value = valueCell.Take(cancellationToken);
                index = NextIndex(index);
                return value;
            }
            finally
            {
                _outgoingIndexCell.Put(index);
            }
        }

        public Task<T> TakeAsync() => TakeAsync(_emptyCancellationToken);
        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            var index = await _outgoingIndexCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                var value = await valueCell.TakeAsync(cancellationToken).ConfigureAwait(false);
                index = NextIndex(index);
                return value;
            }
            finally
            {
                _outgoingIndexCell.Put(index);
            }
        }

        public bool TryPut(T value)
        {
            var success = false;

            int index;
            if (_incomingIndexCell.TryTake().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                if (valueCell.TryPut(value))
                {
                    index = NextIndex(index);
                    success = true;
                }

                _incomingIndexCell.Put(index);
            }

            return success;
        }

        public void Put(T value) => Put(value, _emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken)
        {
            var index = _incomingIndexCell.Take(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                valueCell.Put(value, cancellationToken);
                index = NextIndex(index);
            }
            finally
            {
                _incomingIndexCell.Put(index);
            }
        }

        public Task PutAsync(T value) => PutAsync(value, _emptyCancellationToken);
        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            var index = await _incomingIndexCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                await valueCell.PutAsync(value, cancellationToken).ConfigureAwait(false);
                index = NextIndex(index);
            }
            finally
            {
                _incomingIndexCell.Put(index);
            }
        }

        private int NextIndex(int index) => (index + 1) % _capacity;
    }
}
