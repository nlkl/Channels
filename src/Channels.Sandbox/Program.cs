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
            //TestMVar();
            TestUnboundedChannel();
            Console.ReadKey();
        }

        public static void TestUnboundedChannel()
        {
            Console.WriteLine("= TESTING UNBOUNDED CHANNEL =");

            var completionSignal = new MVar<object>();

            var channel = new UnboundedChannel<int>();

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

        public static void TestMVar()
        {
            Console.WriteLine("= TESTING MVAR =");

            var completionSignal = new MVar<object>();

            var mvar = new MVar<int>();

            var putTask = Task.Run(() =>
            {
                Thread.Sleep(2000);
                PutAndShow(mvar, 10);

                Thread.Sleep(1000);
                PutAndShow(mvar, 20);

                Parallel.Invoke(
                    () => PutAndShow(mvar, 1),
                    () => PutAndShow(mvar, 2),
                    () => PutAndShow(mvar, 3),
                    () => PutAndShow(mvar, 4)
                );

                completionSignal.Put(null);
            });

            TakeAndShow(mvar);
            TakeAndShow(mvar);

            Parallel.Invoke(
                () => TakeAndShow(mvar),
                () => TakeAndShow(mvar),
                () => TakeAndShow(mvar),
                () => TakeAndShow(mvar)
            );

            completionSignal.Take();
            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.WriteLine();
        }

        private static void PutAndShow<T>(IChannel<T> mvar, T value)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Putting value '{value}' from thread {threadId}");
            var sw = Stopwatch.StartNew();
            mvar.Put(value);
            sw.Stop();
            Console.WriteLine($"Successfully put value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        private static void TakeAndShow<T>(IChannel<T> mvar)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Taking value from thread {threadId}");
            var sw = Stopwatch.StartNew();
            var value = mvar.Take();
            sw.Stop();
            Console.WriteLine($"Successfully took value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }
    }
}
