using System.Threading.Tasks;

namespace Channels
{
    public interface ISelectable
    {
        Task WaitTask { get; }
        void Accept();
        void Reject();
    }

    public interface ISelectable<T>
    {
        Task WaitTask { get; }
        T Accept();
        void Reject();
    }
}
