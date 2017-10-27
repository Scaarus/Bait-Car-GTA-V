using System;
using System.Reflection;
using Bait_Car.Handlers;
using LSPD_First_Response.Mod.API;
using Rage;

namespace Bait_Car
{
    public class Main : Plugin
    {
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static ConfigHandler Config;
        public static MenuHandler Menu;
        public static StateHandler State = new StateHandler { State = Bait_Car.State.None };

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        private static void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (!onDuty) return;

            LogHandler.Log("Initializing...");
            Config = new ConfigHandler();
            LogHandler.Log("Configuration loaded.");
            Menu = new MenuHandler(Config, State);
            LogHandler.Log("Menu configured and loaded.");
            Menu.OnMenuItemSelected += MenuEventHandler;
            LogHandler.Log("Events registered.");
            LogHandler.Log("Initialization complete.");
            Game.DisplayNotification($"Bait Car {Version} Loaded!");
        }

        private static void MenuEventHandler(MenuHandlerEventArgs e)
        {
            switch (e.Type)
            {
                case MenuHandlerEventArgs.EventType.SpawnVehicle:
                    break;
                case MenuHandlerEventArgs.EventType.UseCurrentVehicle:
                    break;
                case MenuHandlerEventArgs.EventType.ToggleEngine:
                    break;
                case MenuHandlerEventArgs.EventType.ToggleLocks:
                    break;
                case MenuHandlerEventArgs.EventType.EndSession:
                    break;
                default:
                    LogHandler.Log("Unknown event type: " + e.Type);
                    return;
            }
        }

        public static void Update()
        {
            while (true)
            {
                Menu.Update();
            }
        }

        public override void Finally()
        {
            throw new NotImplementedException();
        }
    }
}
