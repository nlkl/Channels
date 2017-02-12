using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Channels
{
    public static class Channel
    {
        public static T Select<T>(params ISelectableChannel<T>[] channels) => SelectAsync(channels).GetAwaiter().GetResult();
        public static Task<T> SelectAsync<T>(params ISelectableChannel<T>[] channels)
        {
            if (channels == null || channels.Any(channel => channel == null)) throw new ArgumentNullException(nameof(channels));
            var selectables = channels.Select(channel => channel.ReadSelectable()).ToArray();
            return SelectAsync(selectables);
        }

        public static void Select(params ISelectable[] selectables) => SelectAsync(selectables).GetAwaiter().GetResult();
        public static Task SelectAsync(params ISelectable[] selectables)
        {
            if (selectables == null || selectables.Any(selectable => selectable == null)) throw new ArgumentNullException(nameof(selectables));
            var valuedSelectables = selectables.Select(selectable => selectable.ContinueWith(() => default(bool))).ToArray();
            return SelectAsync(valuedSelectables);
        }

        public static T Select<T>(params ISelectable<T>[] selectables) => SelectAsync(selectables).GetAwaiter().GetResult();
        public static async Task<T> SelectAsync<T>(params ISelectable<T>[] selectables)
        {
            if (selectables == null || selectables.Any(selectable => selectable == null)) throw new ArgumentNullException(nameof(selectables));

            await Task.WhenAny(selectables.Select(selectable => selectable.WaitTask));

            var readySelectables = new List<ISelectable<T>>(selectables.Length);
            foreach (var selectable in selectables)
            {
                if (selectable.WaitTask.IsCompleted)
                {
                    readySelectables.Add(selectable);
                }
                else
                {
                    selectable.Reject();
                }
            }

            var random = new Random();
            var selectionIndex = random.Next(0, readySelectables.Count - 1);

            for (int i = 0; i < readySelectables.Count; i++)
            {
                if (i == selectionIndex) continue;
                readySelectables[i].Reject();
            }

            return readySelectables[selectionIndex].Accept();
        }

        public static void Test()
        {
            var channel1 = new UnboundedChannel<int>() as ISelectableChannel<int>;
            var channel2 = new UnboundedChannel<int>() as ISelectableChannel<int>;
        }
    }
}
