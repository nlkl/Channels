using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class BufferedChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly int _capacity;
        private readonly MVar<int> _writeIndexCell;
        private readonly MVar<int> _readIndexCell;
        private readonly MVar<T>[] _buffer;

        public BufferedChannel(int capacity)
        {
            if (capacity < 1) throw new ArgumentException(nameof(capacity), "Buffer must have a capacity greater than zero.");

            _capacity = capacity;
            _writeIndexCell = new MVar<int>(0);
            _readIndexCell = new MVar<int>(0);

            _buffer = new MVar<T>[capacity];
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = new MVar<T>();
            }
        }

        public PotentialValue<T> TryInspect()
        {
            var result = PotentialValue<T>.WithoutValue();

            int index;
            if (_readIndexCell.TryRead().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                T value;
                if (valueCell.TryInspect().TryGetValue(out value))
                {
                    result = PotentialValue<T>.WithValue(value);
                }

                _readIndexCell.Write(index);
            }

            return result;
        }

        public T Inspect() => Inspect(_emptyCancellationToken);
        public T Inspect(CancellationToken cancellationToken)
        {
            var index = _readIndexCell.Read(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                var value = valueCell.Inspect(cancellationToken);
                return value;
            }
            finally
            {
                _readIndexCell.Write(index);
            }
        }

        public Task<T> InspectAsync() => InspectAsync(_emptyCancellationToken);
        public async Task<T> InspectAsync(CancellationToken cancellationToken)
        {
            var index = await _readIndexCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                var value = await valueCell.InspectAsync(cancellationToken).ConfigureAwait(false);
                return value;
            }
            finally
            {
                _readIndexCell.Write(index);
            }
        }

        public PotentialValue<T> TryRead()
        {
            var result = PotentialValue<T>.WithoutValue();

            int index;
            if (_readIndexCell.TryRead().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                T value;
                if (valueCell.TryRead().TryGetValue(out value))
                {
                    index = NextIndex(index);
                    result = PotentialValue<T>.WithValue(value);
                }

                _readIndexCell.Write(index);
            }

            return result;
        }

        public T Read() => Read(_emptyCancellationToken);
        public T Read(CancellationToken cancellationToken)
        {
            var index = _readIndexCell.Read(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                var value = valueCell.Read(cancellationToken);
                index = NextIndex(index);
                return value;
            }
            finally
            {
                _readIndexCell.Write(index);
            }
        }

        public Task<T> ReadAsync() => ReadAsync(_emptyCancellationToken);
        public async Task<T> ReadAsync(CancellationToken cancellationToken)
        {
            var index = await _readIndexCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                var value = await valueCell.ReadAsync(cancellationToken).ConfigureAwait(false);
                index = NextIndex(index);
                return value;
            }
            finally
            {
                _readIndexCell.Write(index);
            }
        }

        public bool TryWrite(T value)
        {
            var success = false;

            int index;
            if (_writeIndexCell.TryRead().TryGetValue(out index))
            {
                var valueCell = _buffer[index];
                if (valueCell.TryWrite(value))
                {
                    index = NextIndex(index);
                    success = true;
                }

                _writeIndexCell.Write(index);
            }

            return success;
        }

        public void Write(T value) => Write(value, _emptyCancellationToken);
        public void Write(T value, CancellationToken cancellationToken)
        {
            var index = _writeIndexCell.Read(cancellationToken);
            try
            {
                var valueCell = _buffer[index];
                valueCell.Write(value, cancellationToken);
                index = NextIndex(index);
            }
            finally
            {
                _writeIndexCell.Write(index);
            }
        }

        public Task WriteAsync(T value) => WriteAsync(value, _emptyCancellationToken);
        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            var index = await _writeIndexCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                await valueCell.WriteAsync(value, cancellationToken).ConfigureAwait(false);
                index = NextIndex(index);
            }
            finally
            {
                _writeIndexCell.Write(index);
            }
        }

        public async Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken)
        {
            var index = await _readIndexCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var valueCell = _buffer[index];
                await valueCell.InspectAsync(cancellationToken).ConfigureAwait(false);

                return new Selectable<T>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var value = valueCell.Read();
                        index = NextIndex(index);
                        return value;
                    }
                    finally
                    {
                        _readIndexCell.Write(index);
                    }
                });
            }
            catch
            {
                _readIndexCell.Write(index);
                throw;
            }
        }

        private int NextIndex(int index) => (index + 1) % _capacity;
    }
}
