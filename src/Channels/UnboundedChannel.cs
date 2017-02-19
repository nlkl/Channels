using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class UnboundedChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly MVar<MVar<Node>> _writeCell;
        private readonly MVar<MVar<Node>> _readCell;

        public UnboundedChannel()
        {
            var stream = new MVar<Node>();
            _writeCell = new MVar<MVar<Node>>(stream);
            _readCell = new MVar<MVar<Node>>(stream);
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
                var next = new MVar<Node>();
                var node = new Node(value, next);
                stream.Write(node);
                _writeCell.Write(next);
                success = true;
            }

            return success;
        }

        public void Write(T value) => Write(value, _emptyCancellationToken);
        public void Write(T value, CancellationToken cancellationToken)
        {
            var stream = _writeCell.Read(cancellationToken);
            var next = new MVar<Node>();
            var node = new Node(value, next);
            stream.Write(node);
            _writeCell.Write(next);
        }

        public Task WriteAsync(T value) => WriteAsync(value, _emptyCancellationToken);
        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            var stream = await _writeCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            var next = new MVar<Node>();
            var node = new Node(value, next);
            stream.Write(node);
            _writeCell.Write(next);
        }

        public async Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken)
        {
            var stream = await _readCell.ReadAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await stream.InspectAsync(cancellationToken).ConfigureAwait(false);

                return new Selectable<T>(async waitUntilSelected =>
                {
                    try
                    {
                        await waitUntilSelected(cancellationToken).ConfigureAwait(false);
                        var node = await stream.ReadAsync().ConfigureAwait(false);
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
