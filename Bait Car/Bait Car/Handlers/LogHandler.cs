using System;
using System.Net;

[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
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

            // [Bait Car v0.1 Alpha] (INFO): Plugin initializing...
            var final = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}][Bait Car {EntryPoint.Version}] ({Enum.GetName(typeof(LogType), type)}): {message}";
            Rage.Game.Console.Print(final);
        }
    }
}
