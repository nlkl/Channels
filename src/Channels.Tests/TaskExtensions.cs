using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels.Tests
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task) { }
    }
}
