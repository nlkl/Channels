using System;
using System.Threading.Tasks;

namespace Channels
{
    internal class Selectable : ISelectable
    {
        private readonly Task _waitTask;
        private readonly Action _accept;
        private readonly Action _reject;

        public Selectable(Task waitTask, Action accept, Action reject)
        {
            if (waitTask == null) throw new ArgumentNullException(nameof(waitTask));
            if (accept == null) throw new ArgumentNullException(nameof(accept));
            if (reject == null) throw new ArgumentNullException(nameof(reject));
            _waitTask = waitTask;
            _accept = accept;
            _reject = reject;
        }

        public Task WaitTask => _waitTask;
        public void Accept() => _accept();
        public void Reject() => _reject();
    }

    internal class Selectable<T> : ISelectable<T>
    {
        private readonly Task _waitTask;
        private readonly Func<T> _accept;
        private readonly Action _reject;

        public Selectable(Task waitTask, Func<T> accept, Action reject)
        {
            if (waitTask == null) throw new ArgumentNullException(nameof(waitTask));
            if (accept == null) throw new ArgumentNullException(nameof(accept));
            if (reject == null) throw new ArgumentNullException(nameof(reject));
            _waitTask = waitTask;
            _accept = accept;
            _reject = reject;
        }

        public Task WaitTask => _waitTask;
        public T Accept() => _accept();
        public void Reject() => _reject();
    }
}
