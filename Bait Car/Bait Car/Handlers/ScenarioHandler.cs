using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using TaskStatus = Rage.TaskStatus;

namespace Bait_Car.Handlers
{
    public class ScenarioHandler
    {
        public Vehicle Car { get; private set; }
        public Ped PoliceDriver { get; private set; }
        public Ped TheifDriver { get; private set; }
        public Blip CarBlip { get; private set; }

        private ConfigHandler config;
        private StateHandler state;
        private Stopwatch timer;
        private int timeToWait;

        public ScenarioHandler(ConfigHandler configHandler, StateHandler stateHandler, string carToSpawn)
        {
            timer = new Stopwatch();
            config = configHandler;
            state = stateHandler;
            SpawnVehicle(carToSpawn.ToUpper());
        }

        /// <summary>
        /// Spawns a vehicle with the given name
        /// </summary>
        /// <param name="carToSpawn">Model name of the vehicle to spawn</param>
        private void SpawnVehicle(string carToSpawn)
        {
            switch (carToSpawn)
            {
                case "RANDOM":
                    Car = new Vehicle(m => !m.IsEmergencyVehicle, World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "CARBONIZZARE":
                    Car = new Vehicle(new Model(carToSpawn), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "COMET":
                    Car = new Vehicle(new Model("COMET2"), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "BALLER":
                    Car = new Vehicle(new Model(carToSpawn), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "DOMINATOR":
                    Car = new Vehicle(new Model(carToSpawn), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "CURRENT":
                    Car = Game.LocalPlayer.Character.CurrentVehicle;
                    break;
            }
        }

        /// <summary>
        /// Spawn a driver to bring the car to the player
        /// </summary>
        private void SpawnDriver()
        {
            // Spawn our driver and set flags so they don't despawn or abandon the vehicle
            PoliceDriver = new Ped(new Model("s_m_m_fiboffice_02"), Car.Position, 0)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };
            // Add our blip so the player knows where the vehicle is
            CarBlip = new Blip(Car) {Color = Color.Yellow};

            BringVehicleToPlayer();
        }

        private void BringVehicleToPlayer()
        {
            //  Put the driver in the vehicle
            if (PoliceDriver != null && Car != null && PoliceDriver.Exists() && Car.Exists())
            {
                PoliceDriver.Tasks.EnterVehicle(Car, 10000, -1, EnterVehicleFlags.WarpIn).WaitForCompletion(1000);

                // Drive to the player
                PoliceDriver.Tasks.ClearImmediately();
                PoliceDriver.Tasks.DriveToPosition(Car, Game.LocalPlayer.Character.Position, 16f,
                    VehicleDrivingFlags.Normal, 10f);

                state.State = State.DrivingToPlayer;
                timer.Start();
            }
            else
                LogHandler.Log("Driver or Car doesn't exist!", LogType.Error);
        }

        private void DisposePoliceDriver()
        {
            if (PoliceDriver == null || !PoliceDriver.Exists()) return;
            PoliceDriver.Tasks.Wander();
            PoliceDriver.IsPersistent = false;
        }

        public void Update()
        {
            switch (state.State)
            {
                case State.DrivingToPlayer:
                    if (Game.IsKeyDown(Keys.Y))
                    {
                        // Teleport the driver closer and allow them to do U-turns
                        PoliceDriver.Tasks.ClearImmediately();
                        Car.Position = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(50f));
                        PoliceDriver.Tasks.DriveToPosition(Car, Game.LocalPlayer.Character.Position, 16f,
                            VehicleDrivingFlags.AllowMedianCrossing, 10f);
                    }

                    // Display the warp notification
                    if (timer.Elapsed.TotalSeconds > 30)
                    {
                        Game.DisplayNotification(
                            "Your bait car is stuck in traffic. Press 'Shift' + 'Y' to warp it closer.");
                        timer.Stop();
                    }

                    // Exit the car and approch the player
                    if (Car.DistanceTo(Game.LocalPlayer.Character.Position) <= 10f)
                    {
                        PoliceDriver.Tasks.ClearImmediately();
                        PoliceDriver.Tasks.LeaveVehicle(Car, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        PoliceDriver.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 5f, 0, 1.8f)
                            .WaitForCompletion();
                        Game.DisplaySubtitle("Here is the car you requested, Sir.");
                        DisposePoliceDriver();
                        state.State = State.PlayerParking;
                    }
                    break;
                case State.PlayerParking:
                    // The player isn't in a car and their last car was the bait car
                    // Assume the player move the car
                    if (!Game.LocalPlayer.Character.IsInAnyVehicle(true) && Game.LocalPlayer.Character.LastVehicle == Car)
                    {
                        // Restart our timer to be reused
                        timer.Restart();
                        // Pick the time we should wait before the theif steals the vehicle
                        timeToWait = new Random().Next(config.GetInteger("Options", "MinSecondsToWait", 30),
                            config.GetInteger("Options", "MaxSecondToWait", 120));
                        // Update our state
                        state.State = State.WaitingForTheif;
                    }
                    break;
                case State.WaitingForTheif:
                    if (timer.Elapsed.TotalSeconds < timeToWait) break;
                    // Find a random ped to take the car
                    // Ped cannot be in car
                    TheifDriver = World.EnumeratePeds()
                        .FirstOrDefault(f => !f.IsInAnyVehicle(true) && f.IsHuman && !f.IsLocalPlayer && f.TravelDistanceTo(Car.Position) < 50f);

                    // If we don't find a suitable ped, tell the user to find a new location
                    if (TheifDriver == null)
                    {
                        if (!config.GetBoolean("Options", "Hardcore"))
                            Game.DisplayNotification("Nobody is taking the bait. Find a more populated area.");
                        state.State = State.PlayerParking;
                    }
                    break;
            }
        }

        /// <summary>
        /// Turn the engine on or off
        /// </summary>
        public void ToggleEngine()
        {
            if (Car.Exists())
                Car.IsEngineOn = !Car.IsEngineOn;
        }

        /// <summary>
        /// Lock the car if it's unlocked
        /// Unlock the car if it's locked
        /// </summary>
        public void ToggleLocks()
        {
            Car.SetLockedForPlayer(Game.LocalPlayer, !Car.IsLockedForPlayer(Game.LocalPlayer));
        }

        /// <summary>
        /// Cleanup the scenario nicely so the player doesn't notice
        /// </summary>
        public void CleanupSafe()
        {
            if (PoliceDriver.Exists())
            {
                // Have the driver drive off or walk off
                if (PoliceDriver.CurrentVehicle == Car)
                    PoliceDriver.Tasks.CruiseWithVehicle(Car, 15f, VehicleDrivingFlags.Normal);
                else
                    PoliceDriver.Tasks.Wander();

                // Disable our flags so the driver can despawn when they are far enough away and out of sight
                PoliceDriver.BlockPermanentEvents = false;
                PoliceDriver.IsPersistent = false;
            }

            if (TheifDriver.Exists())
            {
                // Have the driver drive off or walk off
                if (TheifDriver.CurrentVehicle == Car)
                    TheifDriver.Tasks.CruiseWithVehicle(Car, 15f, VehicleDrivingFlags.Normal);
                else
                    TheifDriver.Tasks.Wander();

                // Disable our flags so the driver can despawn when they are far enough away and out of sight
                TheifDriver.BlockPermanentEvents = false;
                TheifDriver.IsPersistent = false;
            }

            // Allow our car to despawn
            // If it has a driver, they will drive away
            // If it has no driver, it will despawn when the player is away
            if (Car.Exists())
                Car.IsPersistent = false;

            // Remove our blip
            if (CarBlip.Exists())
                CarBlip.Delete();
        }

        /// <summary>
        /// Cleanup the plugin by deleting everything. Used when plugin is forced to unload
        /// </summary>
        public void Cleanup()
        {
            if (PoliceDriver != null && PoliceDriver.Exists())
                PoliceDriver.Delete();
            if (TheifDriver != null && TheifDriver.Exists())
                TheifDriver.Delete();
            if (Car != null && Car.Exists())
                Car.Delete();
            if (CarBlip != null && CarBlip.Exists())
                CarBlip.Delete();
        }
    }
}
