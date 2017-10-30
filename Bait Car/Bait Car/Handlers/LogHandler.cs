using System;
using Rage;

namespace Bait_Car.Handlers
{
    public enum LogType
    {
        Info,
        Error,
        Debug
    };


    public static class LogHandler
    {

#if DEBUG
        private const bool DebugMode = true;
#else
        private const bool DebugMode = false;
#endif

        public static void Log(string message, LogType type = LogType.Info)
        {
            // Don't print debug statements if debug mode is disabled
            if (type == LogType.Debug && !DebugMode) return;
            
            // [LogType] Bait Car: Message
            var final = $"<{Enum.GetName(typeof(LogType), type)}> {message}";
            Game.LogTrivial(final);
        }
    }
}
