using System;

namespace Channels
{
    internal static class SelectableExtensions
    {
        public static ISelectable<TResult> ContinueWith<TResult>(this ISelectable selectable, Func<TResult> continuation)
        {
            if (selectable == null) throw new ArgumentNullException(nameof(selectable));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            return new Selectable<TResult>(selectable.WaitTask, () => { selectable.Accept(); return continuation(); }, selectable.Reject);
        }

        public static ISelectable<TResult> ContinueWith<T, TResult>(this ISelectable<T> selectable, Func<T, TResult> continuation)
        {
            if (selectable == null) throw new ArgumentNullException(nameof(selectable));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            return new Selectable<TResult>(selectable.WaitTask, () => continuation(selectable.Accept()), selectable.Reject);
        }

        public static ISelectable ContinueWith(this ISelectable selectable, Action continuation)
        {
            if (selectable == null) throw new ArgumentNullException(nameof(selectable));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            return new Selectable(selectable.WaitTask, () => { selectable.Accept(); continuation(); }, selectable.Reject);
        }

        public static ISelectable ContinueWith<T>(this ISelectable<T> selectable, Action<T> continuation)
        {
            if (selectable == null) throw new ArgumentNullException(nameof(selectable));
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            return new Selectable(selectable.WaitTask, () => continuation(selectable.Accept()), selectable.Reject);
        }
    }
}
