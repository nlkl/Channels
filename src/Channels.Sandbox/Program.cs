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
            //Console.WriteLine("= TESTING MVAR =");
            //TestChannel(new MVar<int>());
            //TestChannelBound(new MVar<int>());

            //Console.WriteLine("= TESTING UNBOUNDED CHANNEL =");
            //TestChannel(new UnboundedChannel<int>());
            //TestChannelBound(new UnboundedChannel<int>());

            //Console.WriteLine("= TESTING SYNCHRONOUS CHANNEL =");
            //TestChannel(new SynchronousChannel<int>());
            //TestChannelBound(new SynchronousChannel<int>());

            //Console.WriteLine("= TESTING BUFFERED CHANNEL (3) =");
            //TestChannel(new BufferedChannel<int>(3));
            //TestChannelBound(new BufferedChannel<int>(3));

            Console.WriteLine("= TESTING BOUNDED CHANNEL (3) =");
            TestChannel(new BoundedChannel<int>(3));
            TestChannelBound(new BoundedChannel<int>(3));

            Console.ReadKey();
        }

        public static void TestChannel(IChannel<int> channel)
        {
            var completionSignal = new MVar<object>();

            var writeTask = Task.Run(() =>
            {
                Thread.Sleep(2000);

                for (int i = 0; i < 5; i++)
                {
                    WriteAndShow(channel, i);
                }

                Thread.Sleep(2000);

                for (int i = 5; i < 10; i++)
                {
                    WriteAndShow(channel, i);
                }

                completionSignal.Write(null);
            });

            for (int i = 0; i < 10; i++)
            {
                ReadAndShow(channel);
            }

            completionSignal.Read();
            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.WriteLine();
        }

        private static void WriteAndShow<T>(IChannel<T> channel, T value)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"W: Writing value '{value}' from thread {threadId}");
            var sw = Stopwatch.StartNew();
            channel.Write(value);
            sw.Stop();
            Console.WriteLine($"W: Successfully wrote value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        private static void ReadAndShow<T>(IChannel<T> channel)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"R: Reading value from thread {threadId}");
            var sw = Stopwatch.StartNew();
            var value = channel.Read();
            sw.Stop();
            Console.WriteLine($"R: Successfully read value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        public static void TestChannelBound(IChannel<int> channel)
        {
            try
            {
                for (int i = 1; i < 10; i++)
                {
                    var cancellationTokenSource = new CancellationTokenSource(500);
                    channel.Write(i, cancellationTokenSource.Token);
                    Console.WriteLine($"Wrote: {i}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled writes");
            }

            try
            {
                for (int i = 1; i < 15; i++)
                {
                    var cancellationTokenSource = new CancellationTokenSource(500);
                    var value = channel.Read(cancellationTokenSource.Token);
                    Console.WriteLine($"Read: {value}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled reads");
            }

            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.WriteLine();
        }
    }
}
