using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    internal class AsyncBarrier
    {
        private readonly object _lock = new object();
        private readonly int _initialCount;
        private int _remainingCount;
        private SemaphoreSlim _signal;

        public AsyncBarrier(int participants)
        {
            if (participants < 2) throw new ArgumentOutOfRangeException(nameof(participants), "At least two participants required.");

            _initialCount = participants;
            _remainingCount = participants;
            _signal = new SemaphoreSlim(0, participants);
        }

        public bool SignalAndWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var currentSignal = ReleaseAndGetSignal();
            try
            {
                var success = currentSignal.Wait(millisecondsTimeout, cancellationToken);
                if (success) return true;

                lock (_lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    _remainingCount += 1;
                }

                return false;
            }
            catch
            {
                lock (_lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    _remainingCount += 1;
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

                lock (_lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    _remainingCount += 1;
                }

                return false;
            }
            catch
            {
                lock (_lock)
                {
                    if (currentSignal.Wait(0)) return true;
                    _remainingCount += 1;
                }

                throw;
            }
        }

        private SemaphoreSlim ReleaseAndGetSignal()
        {
            lock (_lock)
            {
                _remainingCount -= 1;

                if (_remainingCount == 0)
                {
                    _signal.Release(_initialCount);
                }
                else if (_remainingCount < 0)
                {
                    _remainingCount = _initialCount - 1;
                    _signal = new SemaphoreSlim(0, _initialCount);
                }

                return _signal;
            }
        }
    }
}
