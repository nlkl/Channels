using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels
{
    public struct Result<T>
    {
        private readonly bool success;
        private readonly T value;

        private Result(T value, bool success)
        {
            this.value = value;
            this.success = success;
        }

        public bool Success => success;

        public T Value
        {
            get
            {
                if (!success) throw new InvalidOperationException("Cannot retrieve value from unsuccessful result.");
                return value;
            }
        }

        public bool TryGetValue(out T value)
        {
            if (success)
            {
                value = this.value;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static Result<T> Ok(T value) => new Result<T>(value, true);
        internal static Result<T> Error() => new Result<T>(default(T), false);
    }
}
