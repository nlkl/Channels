using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Channels.Sandbox
{
    public class Program
    {
        public static void Main()
        {
            var semaphore = new SemaphoreSlim(0, 1);


            var ctSource = new CancellationTokenSource();
            var ct = ctSource.Token;
            ctSource.CancelAfter(100);

            var signalled = semaphore.Wait(Timeout.Infinite, ct);
            Console.WriteLine($"Signalled: {signalled}");
            Console.ReadKey();
        }
    }
}
