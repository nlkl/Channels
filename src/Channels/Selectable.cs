using System;
using System.Threading;

namespace Channels
{
    public class Selectable<T>
    {
        private readonly Lazy<T> _selected;

        public Selectable(Func<T> select)
        {
            if (select == null) throw new ArgumentNullException(nameof(select));
            _selected = new Lazy<T>(select, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public T Select() => _selected.Value;
    }
}
