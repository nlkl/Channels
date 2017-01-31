using System;

namespace Channels
{
    public struct PotentialValue<T>
    {
        private static readonly PotentialValue<T> _empty = new PotentialValue<T>(default(T), false);

        private readonly bool _hasValue;
        private readonly T _value;

        private PotentialValue(T value, bool success)
        {
            _value = value;
            _hasValue = success;
        }

        public bool HasValue => _hasValue;

        public T Value
        {
            get
            {
                if (!_hasValue) throw new InvalidOperationException("Potential value is missing and cannot be retrieved.");
                return _value;
            }
        }

        public bool TryGetValue(out T value)
        {
            if (_hasValue)
            {
                value = _value;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static PotentialValue<T> WithValue(T value) => new PotentialValue<T>(value, true);
        internal static PotentialValue<T> WithoutValue() => _empty;
    }
}
