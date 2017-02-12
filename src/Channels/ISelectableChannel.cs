namespace Channels
{
    public interface ISelectableChannel<T>
    {
        ISelectable<T> ReadSelectable();
    }
}
