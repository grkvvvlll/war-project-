namespace Services.Commands
{
    public class ActionGameCommand : IGameCommand
    {
        private readonly Action _execute;
        private readonly Action _undo;

        public string Description { get; }

        public ActionGameCommand(string description, Action execute, Action undo)
        {
            Description = description;
            _execute = execute;
            _undo = undo;
        }

        public void Execute() => _execute();

        public void Undo() => _undo();
    }
}
