namespace Services.Commands
{
    /// <summary>
    /// Инвокер для команд, не требующих undo/redo.
    /// </summary>
    public class SimpleCommandInvoker
    {
        public void Execute(IGameCommand command) => command.Execute();
    }
}
