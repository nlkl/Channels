using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels
{
    public interface IChannel<T>
    {
        T Peek();
        bool TryPeek(out T value);
        Task<T> PeekAsync();

        T Receive();
        bool TryReceive(out T value);
        Task<T> ReceiveAsync();

        void Send(T value);
        bool TrySend(T value);
        Task SendAsync(T value);
    }
}
