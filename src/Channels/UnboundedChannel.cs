using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class UnboundedChannel<T> : IChannel<T>
    {
        private static readonly CancellationToken _emptyCancellationToken = new CancellationToken();

        private readonly MVar<MVar<Node>> _incomingCell;
        private readonly MVar<MVar<Node>> _outgoingCell;

        public UnboundedChannel()
        {
            var stream = new MVar<Node>();
            _incomingCell = new MVar<MVar<Node>>(stream);
            _outgoingCell = new MVar<MVar<Node>>(stream);
        }

        public PotentialValue<T> TryPeek()
        {
            var result = PotentialValue<T>.WithoutValue();

            MVar<Node> stream;
            if (_outgoingCell.TryTake().TryGetValue(out stream))
            {
                Node node;
                if (stream.TryPeek().TryGetValue(out node))
                {
                    result = PotentialValue<T>.WithValue(node.Value);
                }

                _outgoingCell.Put(stream);
            }

            return result;
        }

        public T Peek() => Peek(_emptyCancellationToken);
        public T Peek(CancellationToken cancellationToken)
        {
            var stream = _outgoingCell.Take(cancellationToken);
            try
            {
                var node = stream.Peek(cancellationToken);
                return node.Value;
            }
            finally
            {
                _outgoingCell.Put(stream);
            }
        }

        public Task<T> PeekAsync() => PeekAsync(_emptyCancellationToken);
        public async Task<T> PeekAsync(CancellationToken cancellationToken)
        {
            var stream = await _outgoingCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.PeekAsync(cancellationToken).ConfigureAwait(false);
                return node.Value;
            }
            finally
            {
                _outgoingCell.Put(stream);
            }
        }

        public PotentialValue<T> TryTake()
        {
            var result = PotentialValue<T>.WithoutValue();

            MVar<Node> stream;
            if (_outgoingCell.TryTake().TryGetValue(out stream))
            {
                Node node;
                if (stream.TryTake().TryGetValue(out node))
                {
                    _outgoingCell.Put(node.Next);
                    result = PotentialValue<T>.WithValue(node.Value);
                }
                else
                {
                    _outgoingCell.Put(stream);
                }
            }

            return result;
        }

        public T Take() => Take(_emptyCancellationToken);
        public T Take(CancellationToken cancellationToken)
        {
            var stream = _outgoingCell.Take(cancellationToken);
            try
            {
                var node = stream.Take(cancellationToken);
                _outgoingCell.Put(node.Next);
                return node.Value;
            }
            catch
            {
                _outgoingCell.Put(stream);
                throw;
            }
        }

        public Task<T> TakeAsync() => TakeAsync(_emptyCancellationToken);
        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            var stream = await _outgoingCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.TakeAsync(cancellationToken).ConfigureAwait(false);
                _outgoingCell.Put(node.Next);
                return node.Value;
            }
            catch
            {
                _outgoingCell.Put(stream);
                throw;
            }
        }

        public bool TryPut(T value)
        {
            var success = false;

            MVar<Node> stream;
            if (_incomingCell.TryTake().TryGetValue(out stream))
            {
                var next = new MVar<Node>();
                var node = new Node(value, next);
                stream.Put(node);
                _incomingCell.Put(next);
                success = true;
            }

            return success;
        }

        public void Put(T value) => Put(value, _emptyCancellationToken);
        public void Put(T value, CancellationToken cancellationToken)
        {
            var stream = _incomingCell.Take(cancellationToken);
            var next = new MVar<Node>();
            var node = new Node(value, next);
            stream.Put(node);
            _incomingCell.Put(next);
        }

        public Task PutAsync(T value) => PutAsync(value, _emptyCancellationToken);
        public async Task PutAsync(T value, CancellationToken cancellationToken)
        {
            var stream = await _incomingCell.TakeAsync(cancellationToken).ConfigureAwait(false);
            var next = new MVar<Node>();
            var node = new Node(value, next);
            stream.Put(node);
            _incomingCell.Put(next);
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
