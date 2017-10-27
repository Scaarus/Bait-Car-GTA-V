using System;
using System.Net;
using LSPD_First_Response.Mod.API;
using Rage;

namespace Bait_Car.Handlers
{
    public enum LogType
    {
        INFO,
        ERROR,
        DEBUG
    };


    public static class LogHandler
    {

#if DEBUG
        private const bool DebugMode = true;
#else
        private const bool DebugMode = false;
#endif

        public static void Log(string message, LogType type = LogType.INFO)
        {
            // Don't print debug statements if debug mode is disabled
            if (type == LogType.DEBUG && !DebugMode) return;
            
            // Bait Car (LogType): Message
            var final = $"Bait Car {Main.Version} ({Enum.GetName(typeof(LogType), type)}): {message}";
            Game.LogTrivial(final);
        }
    }
}
