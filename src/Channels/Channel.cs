using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class Channel<T> : IChannel<T>
    {
        private readonly MVar<MVar<Node>> incoming;
        private readonly MVar<MVar<Node>> outgoing;

        public Channel()
        {
            var stream = new MVar<Node>();
            incoming = new MVar<MVar<Node>>(stream);
            outgoing = new MVar<MVar<Node>>(stream);
        }

        public PotentialValue<T> TryPeek()
        {
            var stream = outgoing.TryTake();
            if (!stream.HasValue) return PotentialValue<T>.WithoutValue();

            var value = PotentialValue<T>.WithoutValue();
            var node = stream.Value.TryPeek();
            if (node.HasValue)
            {
                value = PotentialValue<T>.WithValue(node.Value.Value);
            }
            outgoing.Put(stream.Value);

            return value;
        }

        public T Peek()
        {
            var stream = outgoing.Take();
            var node = stream.Peek();
            outgoing.Put(stream);
            return node.Value;
        }

        public T Peek(CancellationToken cancellationToken)
        {
            var stream = outgoing.Take(cancellationToken);
            try
            {
                var node = stream.Peek(cancellationToken);
                return node.Value;
            }
            finally
            {
                outgoing.Put(stream);
            }
        }

        public async Task<T> PeekAsync()
        {
            var stream = await outgoing.TakeAsync().ConfigureAwait(false);
            var node = await stream.PeekAsync().ConfigureAwait(false);
            await outgoing.PutAsync(stream).ConfigureAwait(false);
            return node.Value;
        }

        public async Task<T> PeekAsync(CancellationToken cancellationToken)
        {
            var stream = await outgoing.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.PeekAsync(cancellationToken).ConfigureAwait(false);
                return node.Value;
            }
            finally
            {
                await outgoing.PutAsync(stream).ConfigureAwait(false);
            }
        }

        public PotentialValue<T> TryReceive()
        {
            var stream = outgoing.TryTake();
            if (!stream.HasValue) return PotentialValue<T>.WithoutValue();

            var value = PotentialValue<T>.WithoutValue();
            var node = stream.Value.TryTake();
            if (node.HasValue)
            {
                value = PotentialValue<T>.WithValue(node.Value.Value);
                outgoing.Put(node.Value.Next);
            }
            else
            {
                outgoing.Put(stream.Value);
            }

            return value;
        }

        public T Receive()
        {
            var stream = outgoing.Take();
            var node = stream.Take();
            outgoing.Put(node.Next);
            return node.Value;
        }

        public T Receive(CancellationToken cancellationToken)
        {
            var stream = outgoing.Take(cancellationToken);
            try
            {
                var node = stream.Take(cancellationToken);
                outgoing.Put(node.Next);
                return node.Value;
            }
            catch
            {
                outgoing.Put(stream);
                throw;
            }
        }

        public async Task<T> ReceiveAsync()
        {
            var stream = await outgoing.TakeAsync().ConfigureAwait(false);
            var node = await stream.TakeAsync().ConfigureAwait(false);
            await outgoing.PutAsync(node.Next).ConfigureAwait(false);
            return node.Value;
        }

        public async Task<T> ReceiveAsync(CancellationToken cancellationToken)
        {
            var stream = await outgoing.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var node = await stream.TakeAsync(cancellationToken).ConfigureAwait(false);
                await outgoing.PutAsync(node.Next).ConfigureAwait(false);
                return node.Value;
            }
            catch
            {
                await outgoing.PutAsync(stream).ConfigureAwait(false);
                throw;
            }
        }

        public bool TrySend(T value)
        {
            var stream = incoming.TryTake();
            if (!stream.HasValue) return false;

            var next = new MVar<Node>();
            var node = new Node(value, next);

            var success = stream.Value.TryPut(node);
            if (success)
            {
                incoming.Put(next);
            }
            else
            {
                incoming.Put(stream.Value);
            }

            return success;
        }

        public void Send(T value)
        {
            var stream = incoming.Take();
            var next = new MVar<Node>();
            var node = new Node(value, next);
            stream.Put(node);
            incoming.Put(next);
        }

        public void Send(T value, CancellationToken cancellationToken)
        {
            var stream = incoming.Take(cancellationToken);
            try
            {
                var next = new MVar<Node>();
                var node = new Node(value, next);
                stream.Put(node, cancellationToken);
                incoming.Put(next);
            }
            catch
            {
                incoming.Put(stream);
                throw;
            }
        }

        public async Task SendAsync(T value)
        {
            var stream = await incoming.TakeAsync().ConfigureAwait(false);
            var next = new MVar<Node>();
            var node = new Node(value, next);
            await stream.PutAsync(node).ConfigureAwait(false);
            await incoming.PutAsync(next).ConfigureAwait(false);
        }

        public async Task SendAsync(T value, CancellationToken cancellationToken)
        {
            var stream = await incoming.TakeAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var next = new MVar<Node>();
                var node = new Node(value, next);
                await stream.PutAsync(node, cancellationToken).ConfigureAwait(false);
                await incoming.PutAsync(next).ConfigureAwait(false);
            }
            catch
            {
                await incoming.PutAsync(stream).ConfigureAwait(false);
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
