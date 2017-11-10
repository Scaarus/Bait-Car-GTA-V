using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LSPD_First_Response.Mod.API;
using Rage;

namespace Bait_Car.Handlers
{
    public class ScenarioHandler
    {
        public Vehicle Car { get; private set; }
        public Ped PoliceDriver { get; private set; }
        public Ped TheifDriver { get; private set; }
        public Blip CarBlip { get; private set; }
        public Blip PedBlip { get; private set; }

        private readonly ConfigHandler _config;
        private readonly StateHandler _state;
        private readonly Stopwatch _timer;
        private int _timeToWait;
        private bool _engineOff = false;
        private readonly List<string> _bannedCars = new List<string> {"RUINER3", "MONSTER", "MARSHALL"};
        private LHandle _pursuitHandle;

        public ScenarioHandler(ConfigHandler configHandler, StateHandler stateHandler, string carToSpawn)
        {
            _timer = new Stopwatch();
            _config = configHandler;
            _state = stateHandler;
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
                    Car = new Vehicle(m => !m.IsEmergencyVehicle && !m.IsLawEnforcementVehicle && m.IsCar && !_bannedCars.Contains(m.Name),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "CARBONIZZARE":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "COMET":
                    Car = new Vehicle(new Model("COMET2"),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "BALLER":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "DOMINATOR":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f)))
                    {
                        IsPersistent = true
                    };
                    SpawnDriver();
                    break;
                case "CURRENT":
                    Car = Game.LocalPlayer.Character.CurrentVehicle;
                    Car.IsPersistent = true;
                    _state.State = State.PlayerParking;
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
            PedBlip = new Blip(PoliceDriver) {Color = Color.Blue};

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

                _state.State = State.DrivingToPlayer;
                _timer.Start();
            }
            else
                LogHandler.Log("Driver or Car doesn't exist!", LogType.Error);
        }

        private void DisposePoliceDriver()
        {
            if (PoliceDriver == null || !PoliceDriver.Exists()) return;
            PoliceDriver.Tasks.Wander();
            PoliceDriver.IsPersistent = false;
            PedBlip.Delete();
        }

        public void Update()
        {
            switch (_state.State)
            {
                case State.DrivingToPlayer:
                    var keys = _config.GetKey("Keys", "WarpCar", new[] { Keys.Shift, Keys.Y });
                    var button = _config.GetButton("Buttons", "WarpCar");

                    // Check if keys/buttons are down
                    if (keys.Length > 1 && Game.IsKeyDown(keys[0]) &&
                        Game.IsKeyDown(keys[1]) ||
                        keys.Length == 1 && Game.IsKeyDown(keys[0]) ||
                        Game.IsControllerButtonDown(button))
                    {
                        // Teleport the driver closer and allow them to do U-turns
                        PoliceDriver.Tasks.ClearImmediately();
                        Car.Position = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(50f));
                        PoliceDriver.Tasks.EnterVehicle(Car, 1000, -1, EnterVehicleFlags.WarpIn).WaitForCompletion();
                        PoliceDriver.Tasks.DriveToPosition(Car, Game.LocalPlayer.Character.Position, 16f,
                            VehicleDrivingFlags.AllowMedianCrossing, 10f);
                    }

                    // Display the warp notification
                    if (_timer.Elapsed.TotalSeconds > 30)
                    {
                        if (keys.Length > 1)
                            Game.DisplayNotification(
                                "Your bait car is stuck in traffic. Press " +
                                $"'{keys[0]}' + '{keys[1]}'" +
                                " to warp it closer.");
                        else
                            Game.DisplayNotification(
                                "Your bait car is stuck in traffic. Press " +
                                $"'{keys[0]}'" +
                                " to warp it closer.");
                        _timer.Stop();
                    }

                    // Exit the car and approch the player
                    if (Car.Exists() && PoliceDriver.Exists() &&
                        Car.DistanceTo(Game.LocalPlayer.Character.Position) <= 10f)
                    {
                        PoliceDriver.Tasks.ClearImmediately();
                        PoliceDriver.Tasks.LeaveVehicle(Car, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        PoliceDriver.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 5f, 0, 1.8f)
                            .WaitForCompletion();
                        Game.DisplaySubtitle("Here is the car you requested, Sir.");
                        DisposePoliceDriver();
                        _state.State = State.PlayerParking;
                    }
                    break;
                case State.PlayerParking:
                    // The player isn't in a car and their last car was the bait car
                    // Assume the player move the car
                    if (!Game.LocalPlayer.Character.IsInAnyVehicle(true) &&
                        Game.LocalPlayer.Character.LastVehicle == Car)
                    {
                        // Restart our timer to be reused
                        _timer.Restart();
                        // Pick the time we should wait before the theif steals the vehicle
                        _timeToWait = new Random().Next(_config.GetInteger("Options", "MinSecondsToWait", 30),
                            _config.GetInteger("Options", "MaxSecondToWait", 120));
                        // Update our state
                        _state.State = State.WaitingForTheif;
                    }
                    break;
                case State.WaitingForTheif:
                    if (_timer.Elapsed.TotalSeconds < _timeToWait) break;
                    // Find a random ped to take the car
                    // Ped cannot be in car
                    TheifDriver = World.EnumeratePeds()
                        .FirstOrDefault(f =>
                            !f.IsInAnyVehicle(true) && f.IsHuman && !f.IsLocalPlayer &&
                            f.TravelDistanceTo(Car.Position) < 50f);

                    // If we don't find a suitable ped, tell the user to find a new location
                    if (TheifDriver == null)
                    {
                        if (!_config.GetBoolean("Options", "Hardcore"))
                            Game.DisplayNotification("Nobody is taking the bait. Find a more populated area.");
                        _state.State = State.PlayerParking;
                    }

                    PedBlip = new Blip(TheifDriver) { Color = Color.Red };

                    if (TheifDriver != null && TheifDriver.Exists())
                    {
                        // Clear the driver's tasks
                        TheifDriver.Tasks.ClearImmediately();
                        // Make them face the car (so we can have a proper heading for the next commmand
                        TheifDriver.Face(Car.Position);
                        // Stand away fromt the vehicle
                        TheifDriver.Tasks.GoToOffsetFromEntity(Car, 20000, 10, TheifDriver.Heading, 2)
                            .WaitForCompletion(20000);
                    }

                    // Approach vehicle & look around
                    if (TheifDriver != null && TheifDriver.Exists())
                    {
                        if (TheifDriver.IsFemale)
                            TheifDriver.Tasks.PlayAnimation("amb@code_human_wander_idles@female@idle_a",
                                "idle_c_lookaround", 5000, 1000, 1000, 0, AnimationFlags.None);
                        else
                            TheifDriver.Tasks.PlayAnimation("amb@code_human_wander_idles@male@idle_b",
                                "idle_e_lookaround", 5000, 1000, 1000, 0, AnimationFlags.None);

                        // The player is too close to the car
                        if (TheifDriver.DistanceTo(Game.LocalPlayer.Character.Position) < 25f)
                        {
                            // Reset our ped
                            TheifDriver.Tasks.ClearImmediately();
                            TheifDriver.Tasks.Wander();
                            TheifDriver = null;

                            // Break out and restart the loop so we get a new ped
                            break;
                        }
                    }

                    // The theif is near the car and done a check for the player.
                    // Get in the vehicle and drive off
                    // TODO: Pick where the driver goes
                    // Chop shop?
                    // Their home?
                    // Just a joy ride?
                    if (TheifDriver != null && TheifDriver.Exists())
                    {
                        TheifDriver.Tasks.ClearImmediately();
                        TheifDriver.Tasks.EnterVehicle(Car, 10000, -1, EnterVehicleFlags.None).WaitForCompletion();
                        TheifDriver.Tasks.CruiseWithVehicle(Car, 60, VehicleDrivingFlags.Emergency);
                    }

                    _state.State = State.VehicleStolen;
                    break;
                case State.VehicleStolen:
                    // TODO:
                    // Doesn't work, LSPDFR dll is in subfolder
                    // Need to point to it explicitly

                    // Use rage to detect if it's loaded. If it isn't, just have the theifdriver drive aorund
                    // Maybe start a gta pursuit?

                    // If it is loaded, reference the dll and check if on duty
                    // If on duty, create lspdfr pursuit
                    
                    if (_pursuitHandle == null)
                    {
                        _pursuitHandle = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(_pursuitHandle, TheifDriver);
                    }
                    break;
            }
        }

        /// <summary>
        /// Turn the engine on or off
        /// </summary>
        public void ToggleEngine()
        {
            // If the engine health of the car is close to 0, set it to 1000 restoring the engine
            // If it's closer to 1000, set the health to 0 disabling the engine
            if (Car.Exists())
            {
                Car.EngineHealth = Math.Abs(Car.EngineHealth) < 100 ? 1000 : 0;
            }
        }

        /// <summary>
        /// Lock the car if it's unlocked
        /// Unlock the car if it's locked
        /// </summary>
        public void ToggleLocks()
        {
            // TODO: This is not how to do it
            // Car.SetLockedForPlayer(Game.LocalPlayer, !Car.IsLockedForPlayer(Game.LocalPlayer));
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