using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels.Tests
{
    public static class TaskStarter
    {
        private const int _delay = 100;

        public static async Task RunDelayed(Action action)
        {
            await Task.Delay(_delay).ConfigureAwait(false);
            action();
        }

        public static async Task RunDelayed(Func<Task> action)
        {
            await Task.Delay(_delay).ConfigureAwait(false);
            await action().ConfigureAwait(false);
        }
    }
}
