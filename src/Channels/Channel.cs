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

        private readonly int bound;
        private int count;

        public Channel()
        {
            this.bound = 0;
            this.count = 0;
        }

        public Channel(int bound)
        {
            if (bound <= 0) throw new ArgumentOutOfRangeException(nameof(bound), "Expected bound to be larger than zero.");
            this.bound = bound;
            this.count = 0;
        }

        public T Peek()
        {
            var stream = outgoing.Take();
            var node = stream.Read();
            outgoing.Put(stream);
            return node.Value;
        }

        public Task<T> PeekAsync()
        {
            throw new NotImplementedException();
        }

        public bool TryPeek(out T value)
        {
            throw new NotImplementedException();
        }


        public T Receive()
        {
            var stream = outgoing.Take();
            var node = stream.Take();
            outgoing.Put(node.Next);
            return node.Value;
        }

        public Task<T> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public bool TryReceive(out T value)
        {
            throw new NotImplementedException();
        }

        public void Send(T value)
        {
            var stream = incoming.Take();
            var node = new Node(value, new MVar<Node>());
            stream.Put(node);
            incoming.Put(stream);
        }

        public Task SendAsync(T value)
        {
            throw new NotImplementedException();
        }

        public bool TrySend(T value)
        {
            throw new NotImplementedException();
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
