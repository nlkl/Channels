using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    internal class AsyncBarrier
    {
        private readonly object @lock = new object();
        private readonly int initialCount;
        private int remainingCount;
        private SemaphoreSlim signal;

        public AsyncBarrier(int participants)
        {
            if (participants < 2) throw new ArgumentOutOfRangeException(nameof(participants), "At least two participants required.");

            initialCount = participants;
            remainingCount = participants;
            signal = new SemaphoreSlim(0, participants);
        }

        public bool SignalAndWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var currentSignal = ReleaseAndGetSignal();
            try
            {
                var success = currentSignal.Wait(millisecondsTimeout, cancellationToken);
                if (success) return true;

                lock (@lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    remainingCount += 1;
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                lock (@lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    remainingCount += 1;
                }

                throw;
            }
        }

        public async Task<bool> SignalAndWaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var currentSignal = ReleaseAndGetSignal();
            try
            {
                var success = await currentSignal
                    .WaitAsync(millisecondsTimeout, cancellationToken)
                    .ConfigureAwait(false);

                if (success) return true;

                lock (@lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    remainingCount += 1;
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                lock (@lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    remainingCount += 1;
                }

                throw;
            }
        }

        private SemaphoreSlim ReleaseAndGetSignal()
        {
            lock (@lock)
            {
                remainingCount -= 1;
                if (remainingCount == 0)
                {
                    signal.Release(initialCount);
                }
                else if (remainingCount < 0)
                {
                    remainingCount = initialCount - 1;
                    signal = new SemaphoreSlim(0, initialCount);
                }
                return signal;
            }
        }
    }
}
