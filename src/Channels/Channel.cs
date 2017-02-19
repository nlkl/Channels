using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public static class Channel
    {
        public static IChannel<T> CreateMVar<T>() => new MVar<T>();
        public static IChannel<T> CreateMVar<T>(T value) => new MVar<T>(value);
        public static IChannel<T> CreateSynchronous<T>() => new SynchronousChannel<T>();
        public static IChannel<T> CreateUnbounded<T>() => new UnboundedChannel<T>();
        public static IChannel<T> CreateBounded<T>(int capacity) => new BoundedChannel<T>(capacity);
        public static IChannel<T> CreateBuffered<T>(int capacity) => new BufferedChannel<T>(capacity);

        public static T Select<T>(params ISelectableChannel<T>[] channels) => Select(channels?.AsEnumerable());
        public static T Select<T>(IEnumerable<ISelectableChannel<T>> channels) => SelectAsync(channels).GetAwaiter().GetResult();

        public static Task<T> SelectAsync<T>(params ISelectableChannel<T>[] channels) => SelectAsync(channels?.AsEnumerable());
        public static async Task<T> SelectAsync<T>(IEnumerable<ISelectableChannel<T>> channels)
        {
            if (channels == null) throw new ArgumentNullException(nameof(channels));

            channels = channels.Where(channel => channel != null).ToArray();
            if (!channels.Any()) throw new ArgumentException("At least one non-null channel must be provided.", nameof(channels));

            var reservations = channels
                .Select(channel => new SelectReservation<T>(channel))
                .ToArray();

            await Task.WhenAny(reservations.Select(reservation => reservation.SelectableTask)).ConfigureAwait(false);

            var readyReservations = new List<SelectReservation<T>>(reservations.Length);
            foreach (var reservation in reservations)
            {
                if (reservation.SelectableTask.IsCompleted)
                {
                    readyReservations.Add(reservation);
                }
                else
                {
                    reservation.Cancel();
                }
            }

            var random = new Random();
            var selectionIndex = random.Next(0, readyReservations.Count - 1);

            for (int i = 0; i < readyReservations.Count; i++)
            {
                if (i == selectionIndex) continue;
                readyReservations[i].Cancel();
            }

            var chosenSelectable = await readyReservations[selectionIndex].SelectableTask.ConfigureAwait(false);
            return await chosenSelectable.SelectAsync().ConfigureAwait(false);
        }

        private struct SelectReservation<T>
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            private readonly Task<Selectable<T>> _selectableTask;

            public SelectReservation(ISelectableChannel<T> channel)
            {
                if (channel == null) throw new ArgumentNullException(nameof(channel));
                _cancellationTokenSource = new CancellationTokenSource();
                _selectableTask = channel.ReadSelectableAsync(_cancellationTokenSource.Token);
            }

            public Task<Selectable<T>> SelectableTask => _selectableTask;
            public void Cancel() => _cancellationTokenSource.Cancel();
        }
    }
}
