namespace Core.Interfaces
{
    public interface IRecordingBattleLogger : IBattleLogger
    {
        List<string> Lines { get; }
        void Clear();
    }
}