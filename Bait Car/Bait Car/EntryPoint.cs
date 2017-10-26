using Bait_Car.Handlers;
using Rage;

[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
namespace Bait_Car
{
    public class EntryPoint
    {
        public const string Version = "v0.3a";
        public static ConfigHandler Config;
        public static MenuHandler Menu;
        public static StateHandler State = new StateHandler {State = Bait_Car.State.None};
        private static GameFiber _gameFiber;

        public static void Main()
        {
            _gameFiber = new GameFiber(Update, "BaitCarFiber");

            LogHandler.Log("Initializing...");
            Config = new ConfigHandler();
            LogHandler.Log("Configuration loaded.");
            Menu = new MenuHandler(Config, State);
            LogHandler.Log("Menu configured and loaded.");
            LogHandler.Log("Initialization complete.");
            _gameFiber.Start();
        }

        public static void Update()
        {
            while (true)
            {
                Menu.Update();
                GameFiber.Yield();
            }
        }
    }
}
