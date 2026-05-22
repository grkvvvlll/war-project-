namespace Services.Commands
{
    public class CommandHistory
    {
        private readonly Stack<IGameCommand> _undoStack = new();
        private readonly Stack<IGameCommand> _redoStack = new();
        private readonly List<string> _entries = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public IReadOnlyList<string> Entries => _entries;

        public void Execute(IGameCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            _entries.Add(command.Description);
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            _entries.Add($"Undo: {command.Description}");
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            _entries.Add($"Redo: {command.Description}");
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _entries.Clear();
        }
    }
}
