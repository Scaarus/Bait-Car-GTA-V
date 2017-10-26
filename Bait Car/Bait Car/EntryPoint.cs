using Bait_Car.Handlers;

[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
namespace Bait_Car
{
    public class EntryPoint
    {
        public const string Version = "v0.2 ALPHA";
        public static ConfigHandler Config;
        public static Menu Menu;


        public static void Main()
        {
            LogHandler.Log("Initializing...");
            Config = new ConfigHandler();
            LogHandler.Log("Configuration loaded.");
            Menu = new Menu(Config);
            LogHandler.Log("Menu configured and loaded.");
            LogHandler.Log("Initialization complete.");
        }
    }
}
