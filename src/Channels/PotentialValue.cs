using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels
{
    public struct PotentialValue<T>
    {
        private readonly bool hasValue;
        private readonly T value;

        private PotentialValue(T value, bool success)
        {
            this.value = value;
            this.hasValue = success;
        }

        public bool HasValue => hasValue;

        public T Value
        {
            get
            {
                if (!hasValue) throw new InvalidOperationException("Potential value is missing and cannot be retrieved.");
                return value;
            }
        }

        public bool TryGetValue(out T value)
        {
            if (hasValue)
            {
                value = this.value;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static PotentialValue<T> WithValue(T value) => new PotentialValue<T>(value, true);
        internal static PotentialValue<T> WithoutValue() => new PotentialValue<T>(default(T), false);
    }
}
