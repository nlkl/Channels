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
            var completionSignal = new MVar<object>();

            var mvar = new MVar<int>();

            var putTask = Task.Run(() =>
            {
                Thread.Sleep(2000);
                PutAndShowMVar(mvar, 10);

                Thread.Sleep(1000);
                PutAndShowMVar(mvar, 20);

                Parallel.Invoke(
                    () => PutAndShowMVar(mvar, 1),
                    () => PutAndShowMVar(mvar, 2),
                    () => PutAndShowMVar(mvar, 3),
                    () => PutAndShowMVar(mvar, 4)
                );

                completionSignal.Put(null);
            });

            TakeAndShowMVar(mvar);
            TakeAndShowMVar(mvar);

            Parallel.Invoke(
                () => TakeAndShowMVar(mvar),
                () => TakeAndShowMVar(mvar),
                () => TakeAndShowMVar(mvar),
                () => TakeAndShowMVar(mvar)
            );

            completionSignal.Take();
            Console.WriteLine();
            Console.WriteLine("= DONE =");
            Console.ReadKey();
        }

        private static void PutAndShowMVar<T>(MVar<T> mvar, T value)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Putting value '{value}' from thread {threadId}");
            var sw = Stopwatch.StartNew();
            mvar.Put(value);
            sw.Stop();
            Console.WriteLine($"Successfully put value '{value}' in {sw.ElapsedMilliseconds} ms from thread {threadId}");
        }

        private static void TakeAndShowMVar<T>(MVar<T> mvar)
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
