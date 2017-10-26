using Bait_Car.Handlers;

[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
namespace Bait_Car
{
    public class EntryPoint
    {
        public const string Version = "v0.2 ALPHA";
        public static ConfigHandler Config;

        public static void Main()
        {
            LogHandler.Log("Plugin Initializing...", LogType.DEBUG);
            Config = new ConfigHandler();
        }
    }
}
