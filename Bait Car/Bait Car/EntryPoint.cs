using System.Reflection;
using Bait_Car.Handlers;
using Rage;


[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
namespace Bait_Car
{
    public class EntryPoint
    {
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string Name = "Bait Car";

        private static ConfigHandler _config;
        private static MenuHandler _menu;
        private static readonly StateHandler State = new StateHandler { State = Bait_Car.State.None };
        private static GameFiber _fiber;
        private static ScenarioHandler _scenario;

        public static void Main()
        {
            LogHandler.Log($"Initializing Bait Car Version {Version}...");
            _config = new ConfigHandler();
            LogHandler.Log("Configuration loaded.");
            _menu = new MenuHandler(_config, State);
            LogHandler.Log("Menu configured and loaded.");
            _menu.OnMenuItemSelected += MenuEventHandler;
            LogHandler.Log("Events registered.");
            LogHandler.Log("Initialization complete.");
            Game.DisplayNotification($"Bait Car {Version} Loaded!");

            _fiber = new GameFiber(Update);
            _fiber.Start();
        }

        private static void MenuEventHandler(MenuHandlerEventArgs e)
        {
            switch (e.Type)
            {
                case MenuHandlerEventArgs.EventType.SpawnVehicle:
                    _scenario = new ScenarioHandler(_config, State, e.VehicleType);
                    break;
                case MenuHandlerEventArgs.EventType.UseCurrentVehicle:
                    _scenario = new ScenarioHandler(_config, State, "CURRENT");
                    break;
                case MenuHandlerEventArgs.EventType.ToggleEngine:
                    _scenario.ToggleEngine();
                    break;
                case MenuHandlerEventArgs.EventType.EndSession:
                    _scenario.CleanupSafe();
                    State.State = Bait_Car.State.None;
                    break;
                default:
                    LogHandler.Log("Unknown event type: " + e.Type, LogType.Error);
                    return;
            }
        }

        public static void Update()
        {
            while (true)
            {
                _menu?.Update();
                _scenario?.Update();
                GameFiber.Yield();
            }
        }

        public static void OnUnload(bool isTerminating)
        {
            if (isTerminating)
                _scenario?.Cleanup();
            else
                _scenario?.CleanupSafe();
        }
    }
}
