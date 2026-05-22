using Services.Logging;

namespace Services.Logging
{
    public class LogCleaner
    {
        private readonly RecordingBattleLogger _logger;

        public LogCleaner(RecordingBattleLogger logger)
        {
            _logger = logger;
        }

        public void Clear()
        {
            _logger.Clear();
            ClearLogFile();
        }

        private void ClearLogFile()
        {
            string logPath = Path.Combine(
                AppContext.BaseDirectory,
                "logs",
                "damage-log.txt");

            if (File.Exists(logPath))
            {
                File.WriteAllText(logPath, string.Empty);
            }
        }
    }
}