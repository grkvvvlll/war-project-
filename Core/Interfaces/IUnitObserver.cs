namespace Core.Interfaces
{
    public interface IUnitObserver
    {
        bool IsEnabled { get; set; }
        void Subscribe(IUnit unit);
        void Unsubscribe(IUnit unit);
    }
}