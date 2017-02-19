using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public class Selectable<T>
    {
        private readonly SemaphoreSlim _selectSignal = new SemaphoreSlim(0, 1);
        private readonly Task<T> _selectTask;

        private int _selectSignalCount = 0;

        public Selectable(Func<Task<T>> selectAsync)
        {
            if (selectAsync == null) throw new ArgumentNullException(nameof(selectAsync));
            _selectSignalCount = 1;
            _selectTask = selectAsync();
        }

        public Selectable(Func<Func<CancellationToken, Task>, Task<T>> selectAsync)
        {
            if (selectAsync == null) throw new ArgumentNullException(nameof(selectAsync));
            _selectTask = selectAsync(cancellationToken => _selectSignal.WaitAsync(cancellationToken));
        }

        public Task<T> SelectAsync()
        {
            if (Interlocked.CompareExchange(ref _selectSignalCount, 1, 0) == 0)
            {
                _selectSignal.Release();
            }

            return _selectTask;
        }
    }
}
