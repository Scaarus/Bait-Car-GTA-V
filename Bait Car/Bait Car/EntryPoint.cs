using System;
using System.Reflection;
using Bait_Car.Handlers;
using Rage;


[assembly: Rage.Attributes.Plugin("Bait Car", Description = "Allows user to spawn bait cars and have AI attempt to steal them.", Author = "Scaarus")]
namespace Bait_Car
{
    public class EntryPoint
    {
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static ConfigHandler Config;
        public static MenuHandler Menu;
        public static StateHandler State = new StateHandler { State = Bait_Car.State.None };
        public static GameFiber Fiber;

        public static void Main()
        {
            LogHandler.Log("Initializing...");
            Config = new ConfigHandler();
            LogHandler.Log("Configuration loaded.");
            Menu = new MenuHandler(Config, State);
            LogHandler.Log("Menu configured and loaded.");
            Menu.OnMenuItemSelected += MenuEventHandler;
            LogHandler.Log("Events registered.");
            LogHandler.Log("Initialization complete.");
            Game.DisplayNotification($"Bait Car {Version} Loaded!");

            Fiber = new GameFiber(Update);
            Fiber.Start();
        }

        private static void MenuEventHandler(MenuHandlerEventArgs e)
        {
            switch (e.Type)
            {
                case MenuHandlerEventArgs.EventType.SpawnVehicle:
                    State.State = Bait_Car.State.DrivingToPlayer;
                    break;
                case MenuHandlerEventArgs.EventType.UseCurrentVehicle:
                    State.State = Bait_Car.State.PlayerParking;
                    break;
                case MenuHandlerEventArgs.EventType.ToggleEngine:
                    State.State = Bait_Car.State.SuspectStopped;
                    break;
                case MenuHandlerEventArgs.EventType.ToggleLocks:
                    break;
                case MenuHandlerEventArgs.EventType.EndSession:
                    State.State = Bait_Car.State.SuspectStopped;
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
                Menu.Update();

                if (State.State == Bait_Car.State.SuspectStopped)
                {
                    State.State = Bait_Car.State.None;
                    break;
                }
                GameFiber.Yield();
            }
        }

        public static void OnUnload()
        {
            throw new NotImplementedException();
        }
    }
}
