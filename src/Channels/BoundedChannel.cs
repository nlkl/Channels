using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class BoundedChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();
        private static readonly Task _emptyTask = Task.FromResult(0);

        private readonly MVar<MVar<Node>> _writeCell;
        private readonly MVar<MVar<Node>> _readCell;

        private readonly SemaphoreSlim _capacityAvailableSignal = new SemaphoreSlim(0, 1);
        private readonly int _capacity;
        private int _count;

        public BoundedChannel(int capacity)
        {
            if (capacity < 1) throw new ArgumentException(nameof(capacity), "Buffer must have a capacity greater than zero.");

            var stream = new MVar<Node>();
            _writeCell = new MVar<MVar<Node>>(stream);
            _readCell = new MVar<MVar<Node>>(stream);

            _capacity = capacity;
            _count = 0;
        }

        public PotentialValue<T> TryInspect()
        {
            var result = PotentialValue<T>.WithoutValue();

            MVar<Node> stream;
            if (_readCell.TryRead().TryGetValue(out stream))
            {
                Node node;
                if (stream.TryInspect().TryGetValue(out node))
                {
                    result = PotentialValue<T>.WithValue(node.Value);
                }

                _readCell.Write(stream);
            }

            return result;
        }

        public T Inspect() => Inspect(_emptyCancellationToken);
        public T Inspect(CancellationToken cancellationToken)
        {
            var stream = _readCell.Read(cancellationToken);
            try
            {
                var node = stream.Inspect(cancellationToken);
                return node.Value;
            }
            finally
            {
                _readCell.Write(stream);
            }
        }

        public Task<T> InspectAsync() => InspectAsync(_emptyCancellationToken);
        public async Task<T> InspectAsync(CancellationToken cancellationToken)
        {
            var stream = await _readCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.InspectAsync(cancellationToken).ConfigureAwait(false);
                return node.Value;
            }
            finally
            {
                _readCell.Write(stream);
            }
        }

        public PotentialValue<T> TryRead()
        {
            var result = PotentialValue<T>.WithoutValue();

            MVar<Node> stream;
            if (_readCell.TryRead().TryGetValue(out stream))
            {
                Node node;
                if (stream.TryRead().TryGetValue(out node))
                {
                    Decrement();
                    _readCell.Write(node.Next);
                    result = PotentialValue<T>.WithValue(node.Value);
                }
                else
                {
                    _readCell.Write(stream);
                }
            }

            return result;
        }

        public T Read() => Read(_emptyCancellationToken);
        public T Read(CancellationToken cancellationToken)
        {
            var stream = _readCell.Read(cancellationToken);
            try
            {
                var node = stream.Read(cancellationToken);
                Decrement();
                _readCell.Write(node.Next);
                return node.Value;
            }
            catch
            {
                _readCell.Write(stream);
                throw;
            }
        }

        public Task<T> ReadAsync() => ReadAsync(_emptyCancellationToken);
        public async Task<T> ReadAsync(CancellationToken cancellationToken)
        {
            var stream = await _readCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.ReadAsync(cancellationToken).ConfigureAwait(false);
                Decrement();
                _readCell.Write(node.Next);
                return node.Value;
            }
            catch
            {
                _readCell.Write(stream);
                throw;
            }
        }

        public bool TryWrite(T value)
        {
            var success = false;

            MVar<Node> stream;
            if (_writeCell.TryRead().TryGetValue(out stream))
            {
                if (IncrementAndTryWait())
                {
                    var next = new MVar<Node>();
                    var node = new Node(value, next);
                    stream.Write(node);
                    _writeCell.Write(next);
                    success = true;
                }
                else
                {
                    RevertIncrement();
                    _writeCell.Write(stream);
                }
            }

            return success;
        }

        public void Write(T value) => Write(value, _emptyCancellationToken);
        public void Write(T value, CancellationToken cancellationToken)
        {
            var stream = _writeCell.Read(cancellationToken);
            try
            {
                IncrementAndWait(cancellationToken);
                var next = new MVar<Node>();
                var node = new Node(value, next);
                stream.Write(node);
                _writeCell.Write(next);
            }
            catch
            {
                RevertIncrement();
                _writeCell.Write(stream);
                throw;
            }
        }

        public Task WriteAsync(T value) => WriteAsync(value, _emptyCancellationToken);
        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            var stream = await _writeCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await IncrementAndWaitAsync(cancellationToken).ConfigureAwait(false);
                var next = new MVar<Node>();
                var node = new Node(value, next);
                stream.Write(node);
                _writeCell.Write(next);
            }
            catch
            {
                RevertIncrement();
                _writeCell.Write(stream);
                throw;
            }
        }

        public async Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken)
        {
            var stream = await _readCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await stream.InspectAsync(cancellationToken).ConfigureAwait(false);

                return new Selectable<T>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var node = stream.Read();
                        Decrement();
                        _readCell.Write(node.Next);
                        return node.Value;
                    }
                    catch
                    {
                        _readCell.Write(stream);
                        throw;
                    }
                });
            }
            catch
            {
                _readCell.Write(stream);
                throw;
            }
        }

        private void Decrement()
        {
            if (Interlocked.Decrement(ref _count) == _capacity)
            {
                _capacityAvailableSignal.Release();
            }
        }

        private bool IncrementAndTryWait()
        {
            if (Interlocked.Increment(ref _count) > _capacity)
            {
                return _capacityAvailableSignal.Wait(0);
            }
            return true;
        }

        private void IncrementAndWait(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _count) > _capacity)
            {
                _capacityAvailableSignal.Wait(cancellationToken);
            }
        }

        private Task IncrementAndWaitAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _count) > _capacity)
            {
                return _capacityAvailableSignal.WaitAsync(cancellationToken);
            }
            return _emptyTask;
        }

        private void RevertIncrement()
        {
            if (Interlocked.Decrement(ref _count) < _capacity)
            {
                _capacityAvailableSignal.Wait();
            }
        }

        private struct Node
        {
            public T Value { get; }
            public MVar<Node> Next { get; }

            public Node(T value, MVar<Node> next)
            {
                Value = value;
                Next = next;
            }
        }
    }
}
