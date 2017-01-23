using System.Threading;
using System.Threading.Tasks;

namespace Channels
{
    public interface IChannel<T> : IPuttable<T>, ITakeable<T>, IPeekable<T>
    {
    }
}
