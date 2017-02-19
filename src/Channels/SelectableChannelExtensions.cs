using System;
using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public static class SelectableChannelExtensions
    {
        public static ISelectableChannel<TResult> SelectWith<T, TResult>(this ISelectableChannel<T> channel, Func<T, TResult> continuation)
        {
            return channel.SelectWith(value => Task.FromResult(continuation(value)));
        }

        public static ISelectableChannel<TResult> SelectWith<T, TResult>(this ISelectableChannel<T> channel, Func<T, Task<TResult>> continuation)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));

            return new SelectableChannelContinuation<TResult>(async cancellationToken =>
            {
                var selectable = await channel.ReadSelectableAsync(cancellationToken).ConfigureAwait(false);

                return new Selectable<TResult>(async () =>
                {
                    var selectedValue = await selectable.SelectAsync().ConfigureAwait(false);
                    return await continuation(selectedValue).ConfigureAwait(false);
                });
            });
        }

        private class SelectableChannelContinuation<T> : ISelectableChannel<T>
        {
            private readonly Func<CancellationToken, Task<Selectable<T>>> _readSelectableAsync;

            public SelectableChannelContinuation(Func<CancellationToken, Task<Selectable<T>>> readSelectableAsync)
            {
                if (readSelectableAsync == null) throw new ArgumentNullException(nameof(readSelectableAsync));
                _readSelectableAsync = readSelectableAsync;
            }

            public Task<Selectable<T>> ReadSelectableAsync(CancellationToken cancellationToken) => _readSelectableAsync(cancellationToken);
        }
    }
}
