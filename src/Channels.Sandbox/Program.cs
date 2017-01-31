using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("= TESTING MVAR =");
            //TestChannel(new MVar<int>());
            TestChannelBound(new MVar<int>());

            Console.WriteLine("= TESTING UNBOUNDED CHANNEL =");
            //TestChannel(new UnboundedChannel<int>());
            TestChannelBound(new UnboundedChannel<int>());

            Console.WriteLine("= TESTING SYNCHRONOUS CHANNEL =");
            //TestChannel(new SynchronousChannel<int>());
            TestChannelBound(new SynchronousChannel<int>());

            Console.WriteLine("= TESTING BUFFERED CHANNEL (3) =");
            //TestChannel(new BufferedChannel<int>(3));
            TestChannelBound(new BufferedChannel<int>(3));

            Console.ReadKey();
        }

        public static void TestChannel(IChannel<int> channel)
        {
            var completionSignal = new MVar<object>();

            var putTask = Task.Run(() =>
            {
                Thread.Sleep(2000);

                for (int i = 0; i < 5; i++)
                {
                    PutAndShow(channel, i);
                }

                Thread.Sleep(2000);

                for (int i = 5; i < 10; i++)
                {
                    PutAndShow(channel, i);
                }

                completionSignal.Put(null);
            });

            for (int i = 0; i < 10; i++)
            {
                TakeAndShow(channel);
            }

            completionSignal.Take();
            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.WriteLine();
        }

        private static void PutAndShow<T>(IChannel<T> mvar, T value)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"P: Putting value '{value}' from thread {threadId}");
            var sw = Stopwatch.StartNew();
            mvar.Put(value);
            sw.Stop();
            Console.WriteLine($"P: Successfully put value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        private static void TakeAndShow<T>(IChannel<T> mvar)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"T: Taking value from thread {threadId}");
            var sw = Stopwatch.StartNew();
            var value = mvar.Take();
            sw.Stop();
            Console.WriteLine($"T: Successfully took value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        public static void TestChannelBound(IChannel<int> channel)
        {
            try
            {
                for (int i = 1; i < 10; i++)
                {
                    var cancellationTokenSource = new CancellationTokenSource(500);
                    channel.Put(i, cancellationTokenSource.Token);
                    Console.WriteLine($"Put: {i}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled puts");
            }

            try
            {
                for (int i = 1; i < 10; i++)
                {
                    var cancellationTokenSource = new CancellationTokenSource(500);
                    var value = channel.Take(cancellationTokenSource.Token);
                    Console.WriteLine($"Took: {value}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled takes");
            }

            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.WriteLine();
        }
    }
}
