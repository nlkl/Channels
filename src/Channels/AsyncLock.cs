using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    internal class AsyncLock
    {
        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

        public void Synchronize(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            mutex.Wait();
            try
            {
                action();
            }
            finally
            {
                mutex.Release();
            }
        }

        public T Synchronize<T>(Func<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            mutex.Wait();
            try
            {
                return action();
            }
            finally
            {
                mutex.Release();
            }
        }

        public async Task SynchronizeAsync(Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            await mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await action().ConfigureAwait(false);
            }
            finally
            {
                mutex.Release();
            }
        }

        public async Task<T> SynchronizeAsync<T>(Func<Task<T>> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            await mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                return await action().ConfigureAwait(false);
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}
