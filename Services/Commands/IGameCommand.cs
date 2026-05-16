namespace Services.Commands
{
    public interface IGameCommand
    {
        string Description { get; }
        void Execute();
        void Undo();
    }
}
