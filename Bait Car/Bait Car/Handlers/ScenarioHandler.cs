﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rage;

namespace Bait_Car.Handlers
{
    public class ScenarioHandler
    {
        public Vehicle Car { get; private set; }
        public Ped PoliceDriver { get; private set; }
        public Ped TheifDriver { get; private set; }
        public Blip CarBlip { get; private set; }

        private readonly ConfigHandler _config;
        private readonly StateHandler _state;
        private readonly Stopwatch _timer;
        private int _timeToWait;
        private bool _engineDisabled;
        private Keys[] _keys;
        private ControllerButtons _button;

        public readonly List<string> Vehicles = new List<string>
        {
            "BLISTA",
            "BRIOSO",
            "DILETTANTE",
            "ISSI2",
            "PANTO",
            "PRAIRIE",
            "RHAPSODY",
            "COGCABRIO",
            "EXEMPLAR",
            "F620",
            "FELON",
            "FELON2",
            "JACKAL",
            "ORACLE",
            "ORACLE2",
            "SENTINEL",
            "SENTINEL2",
            "WINDSOR",
            "WINDSOR2",
            "ZION",
            "ZION2",
            "BLADE",
            "BUCCANEER",
            "BUCCANEER2",
            "CHINO",
            "CHINO2",
            "COQUETTE3",
            "DOMINATOR",
            "DUKES",
            "FACTION",
            "FACTION2",
            "GAUNTLET",
            "MOONBEAM",
            "MOONBEAM2",
            "NIGHTSHADE",
            "NIGHTSHADE2",
            "PHOENIX",
            "PICADOR",
            "RATOLOADER2",
            "RUINER",
            "SABREGT",
            "SABREGT2",
            "SLAMVAN",
            "SLAMVAN3",
            "STALION",
            "TAMPA",
            "VIGERO",
            "VIRGO",
            "VIRGO3",
            "VIRGO2",
            "VOODOO",
            "ASEA",
            "ASTEROPE",
            "COG55",
            "COGNOSCENTI",
            "EMPORER",
            "FUGITIVE",
            "GLENDALE",
            "INGOT",
            "INTRUDER",
            "PREMIER",
            "PRIMO",
            "PRIMO2",
            "REGINA",
            "ROMERO",
            "SCHAFTER2",
            "STANIER",
            "STRATUM",
            "SUPERD",
            "SURGE",
            "TAILGATER",
            "WARRENER",
            "WASHINGTON",
            "ALPHA",
            "BANSHEE",
            "BESTIAGTS",
            "BUFFALO",
            "BUFFALO2",
            "CARBONIZZARE",
            "COMET2",
            "COMET3",
            "COQUETTE",
            "ELEGY",
            "ELEGY2",
            "FELTZER2",
            "FUROREGT",
            "FUSILADE",
            "FUTO",
            "BLISTA2",
            "JESTER",
            "KHAMELION",
            "KURUMA",
            "LYNX",
            "MASSACRO",
            "NINEF",
            "NINEF2",
            "PENUMBRA",
            "RAPIDGT",
            "SCHAFTER4",
            "SCHAFTER3",
            "SCHWARZER",
            "SEVEN70",
            "SPECTER",
            "SULTAN",
            "SURANO",
            "VERLIERER2",
            "BALLER",
            "BALLER2",
            "BALLER3",
            "BJXL",
            "CAVALCADE",
            "CAVALCADE2",
            "CONTENDER",
            "DUBSTA",
            "DUBSTA2",
            "FQ2",
            "GRANGER",
            "GRESLEY",
            "HABANERO",
            "HUNTLEY",
            "LANDSTALKER",
            "MESA",
            "PATRIOT",
            "RADI",
            "ROCOTO",
            "SEMINOLE",
            "SERRANO",
            "XLS",
        };

        public ScenarioHandler(ConfigHandler configHandler, StateHandler stateHandler, string carToSpawn)
        {
            _timer = new Stopwatch();
            _config = configHandler;
            _state = stateHandler;

            _state.Event += StateChangedHandler;

            _keys = _config.GetKey("Keys", "WarpCar", new[] { Keys.Shift, Keys.Y });
            _button = _config.GetButton("Controller", "WarpCar");

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
                    Car = new Vehicle(new Model(Vehicles[new Random().Next(Vehicles.Count)]),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200)))
                    {
                        IsPersistent = true
                    };
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    DriveToPlayer();
                    break;
                case "CARBONIZZARE":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200)))
                    {
                        IsPersistent = true
                    };
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    DriveToPlayer();
                    break;
                case "COMET":
                    Car = new Vehicle(new Model("COMET2"),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200)))
                    {
                        IsPersistent = true
                    };
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    DriveToPlayer();
                    break;
                case "BALLER":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200)))
                    {
                        IsPersistent = true
                    };
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    DriveToPlayer();
                    break;
                case "DOMINATOR":
                    Car = new Vehicle(new Model(carToSpawn),
                        World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200)))
                    {
                        IsPersistent = true
                    };
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    DriveToPlayer();
                    break;
                case "CURRENT":
                    Car = Game.LocalPlayer.Character.CurrentVehicle;
                    Car.IsPersistent = true;
                    // Add our blip so the player knows where the vehicle is
                    CarBlip = new Blip(Car) { Color = Color.Yellow };
                    _state.State = State.PlayerParking;
                    break;
            }
        }

        private void DriveToPlayer()
        {
            // Spawn our driver and set flags so they don't despawn or abandon the vehicle
            PoliceDriver = new Ped(new Model("s_m_m_fiboffice_02"), Car.Position, 0)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            if (Car)
            {
                // Warp the driver into the car
                PoliceDriver.Tasks.ClearImmediately();
                PoliceDriver.Tasks.EnterVehicle(Car, 1000, -1, EnterVehicleFlags.WarpIn).WaitForCompletion(1000);

                // Drive to the player
                PoliceDriver.Tasks.DriveToPosition(Car, Game.LocalPlayer.Character.Position, 16f,
                    VehicleDrivingFlags.Normal, 10f);

                _state.State = State.DrivingToPlayer;
                _timer.Start();
            }
        }

        /// <summary>
        /// Makes the police driver wander and removes flags so they can naturally despawn
        /// </summary>
        private void DisposePoliceDriver()
        {
            if (!PoliceDriver) return;
            PoliceDriver.Tasks.Clear();
            PoliceDriver.Tasks.Wander();
            PoliceDriver.IsPersistent = false;
            PoliceDriver = null;
        }

        /// <summary>
        /// Checks if the bound warp keys or buttons are pressed
        /// </summary>
        /// <returns>True if the buttons are pressed</returns>
        private bool AreWarpKeysPressed()
        {
            bool keysPressed;

            switch (_keys.First())
            {
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    keysPressed = Game.IsShiftKeyDownRightNow;
                    break;
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    keysPressed = Game.IsControlKeyDownRightNow;
                    break;
                default:
                    keysPressed = Game.IsKeyDown(_keys.First());
                    break;
            }

            switch (_keys.Last())
            {
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    if (keysPressed)
                        keysPressed = Game.IsShiftKeyDownRightNow;
                    break;
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    if (keysPressed)
                        keysPressed = Game.IsControlKeyDownRightNow;
                    break;
                default:
                    keysPressed = Game.IsKeyDown(_keys.Last());
                    break;
            }

            return keysPressed || Game.IsControllerButtonDown(_button);
        }

        /// <summary>
        /// Checks if the bound kill engine keys or buttons are pressed
        /// </summary>
        /// <returns>True if the buttons are pressed</returns>
        private bool AreEngineKeysPressed()
        {
            var keys = _config.GetKey("Keys", "KillSwitch", new[] { Keys.Shift, Keys.K });
            var button = _config.GetButton("Controller", "KillSwitch");

            bool keysPressed;

            switch (keys.First())
            {
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    keysPressed = Game.IsShiftKeyDownRightNow;
                    break;
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    keysPressed = Game.IsControlKeyDownRightNow;
                    break;
                default:
                    keysPressed = Game.IsKeyDown(keys.First());
                    break;
            }

            switch (keys.Last())
            {
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    if (keysPressed)
                        keysPressed = Game.IsShiftKeyDownRightNow;
                    break;
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    if (keysPressed)
                        keysPressed = Game.IsControlKeyDownRightNow;
                    break;
                default:
                    keysPressed = Game.IsKeyDown(keys.Last());
                    break;
            }

            return keysPressed || Game.IsControllerButtonDown(button);
        }

        /// <summary>
        /// Calculates the heading for the ped to face the given vehicle.
        /// </summary>
        /// <param name="ped">Ped to change heading for.</param>
        /// <param name="entityToLookAt">Entity to look at.</param>
        /// <returns>The heading to look at the vehicle.</returns>
        public double CalculateHeading(Ped ped, Entity entityToLookAt)
        {
            return Math.Atan2(ped.Position.Y - entityToLookAt.Position.Y, ped.Position.X - entityToLookAt.Position.X);
        }

        /// <summary>
        /// Make the thief drive to a location or cruise
        /// </summary>
        public void DriveToLocation()
        {
            if (TheifDriver)
            {
                TheifDriver.Tasks.EnterVehicle(Car, 10000, -1, EnterVehicleFlags.AllowJacking).WaitForCompletion();
                TheifDriver.Tasks.CruiseWithVehicle(Car, 13, VehicleDrivingFlags.Normal);
            }
        }

        /// <summary>
        /// The main update loop, handle states.
        /// </summary>
        public void Update()
        {
            switch (_state.State)
            {
                case State.DrivingToPlayer:
                    // Exit the car and approch the player
                    if (Car && PoliceDriver &&
                        Car.DistanceTo(Game.LocalPlayer.Character.Position) <= 10f)
                    {
                        PoliceDriver.Tasks.Clear();
                        PoliceDriver.Tasks.LeaveVehicle(Car, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        PoliceDriver.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 5f, (float)CalculateHeading(PoliceDriver, Game.LocalPlayer.Character), 1.8f)
                            .WaitForCompletion(1000);
                        Game.DisplaySubtitle("Here is the car you requested, Sir.");
                        DisposePoliceDriver();
                        _state.State = State.PlayerParking;
                    }

                    // Check if keys/buttons are down
                    if (AreWarpKeysPressed())
                    {
                        LogHandler.Log("Warp keys pressed", LogType.Debug);

                        // Teleport the driver closer and allow them to do U-turns
                        PoliceDriver.Tasks.Clear();
                        Car.Position = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(50f));
                        PoliceDriver.Tasks.EnterVehicle(Car, 1000, -1, EnterVehicleFlags.WarpIn).WaitForCompletion();
                        PoliceDriver.Tasks.DriveToPosition(Car, Game.LocalPlayer.Character.Position, 16f,
                            VehicleDrivingFlags.AllowMedianCrossing, 10f);
                    }

                    // Display the warp notification
                    if (_timer.Elapsed.TotalSeconds > 30)
                    {
                        // TODO: Display button if set and using controller
                        if (_keys.Length > 1)
                            Game.DisplayNotification(
                                "Your bait car is stuck in traffic. Press " +
                                $"~y~'{_keys[0]} + {_keys[1]}'~w~" +
                                " to warp it closer.");
                        else
                            Game.DisplayNotification(
                                "Your bait car is stuck in traffic. Press " +
                                $"~y~'{_keys[0]}'~w~" +
                                " to warp it closer.");
                        _timer.Reset();
                    }
                    break;

                case State.PlayerParking:
                    // The player isn't in a car and their last car was the bait car
                    // Assume the player moved the car
                    if (!Game.LocalPlayer.Character.IsInAnyVehicle(true) &&
                        Game.LocalPlayer.Character.LastVehicle == Car)
                    {
                        // Restart our timer to be reused
                        _timer.Restart();
                        // Pick the time we should wait before the theif steals the vehicle
                        _timeToWait = new Random().Next(_config.GetInteger("Options", "MinSecondsToWait", 30),
                            _config.GetInteger("Options", "MaxSecondsToWait", 120));

                        LogHandler.Log($"Time to wait: {_timeToWait}", LogType.Debug);

                        // Update our state
                        _state.State = State.WaitingForTheif;
                    }
                    break;

                case State.WaitingForTheif:
                    // Wait until our timer passes than random time to wait
                    if (_timer.Elapsed.TotalSeconds < _timeToWait) break;

                    // Find a random ped to take the car
                    // Ped cannot be in car
                    TheifDriver = World.EnumeratePeds()
                        .FirstOrDefault(f =>
                            !f.IsInAnyVehicle(true) && f.IsHuman && !f.IsLocalPlayer &&
                            f.TravelDistanceTo(Car.Position) < 50f);

                    // If we don't find a suitable ped, tell the user to find a new location
                    if (!TheifDriver)
                    {
                        LogHandler.Log("Unable to find thief driver", LogType.Debug);
                        if (!_config.GetBoolean("Options", "Hardcore"))
                            Game.DisplayNotification("Nobody is taking the bait. Find a more populated area.");
                        _state.State = State.PlayerParking;
                        break;
                    }

                    if (TheifDriver)
                    {
                        // 1 in 8 chance of theif having gun
                        if (new Random().Next(0, 8) == 2)
                            TheifDriver.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PISTOL50"), 1000, true);
                        // Clear the driver's tasks
                        TheifDriver.Tasks.Clear();
                        // Stand away from the vehicle
                        TheifDriver.Tasks.GoStraightToPosition(Car.Position, 1, (float)CalculateHeading(TheifDriver, Car), 3, 30000).WaitForCompletion();
                    }

                    // Approach vehicle & look around
                    if (TheifDriver)
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
                            DisposePoliceDriver();

                            // Restart our wait timer and break out of the loop so we can find a new ped
                            _timer.Restart();
                            break;
                        }
                    }

                    TheifDriver.Tasks.Clear();
                    DriveToLocation();
                    if (!_config.GetBoolean("Options", "Hardcore"))
                        Game.DisplayNotification("The bait car has been taken!");

                    _state.State = State.VehicleStolen;
                    break;

                case State.VehicleStolen:
                    if (AreEngineKeysPressed())
                        ToggleEngine();

                    if (_engineDisabled && TheifDriver.Tasks.CurrentTaskStatus == TaskStatus.None &&
                        TheifDriver.IsInAnyVehicle(true))
                    {
                        TheifDriver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                    }

                    // TODO: Monitor for following police vehicles
                    if (!TheifDriver.IsInAnyVehicle(true) || TheifDriver.IsDead)
                        _state.State = State.SuspectStopped;

                    break;


            }
        }

        private void StateChangedHandler(StateHandler s, State previousState, State newState)
        {
            switch (newState)
            {

                case State.SuspectStopped:
                    CleanupSafe();
                    _state.State = State.None;
                    break;
            }
        }

        /// <summary>
        /// Turn the engine on or off
        /// </summary>
        public void ToggleEngine()
        {
            _engineDisabled = !_engineDisabled;

            if (TheifDriver)
            {
                if (!_engineDisabled)
                    DriveToLocation();
                else
                    TheifDriver.Tasks.Clear();
            }

            if (Car)
            {
                Car.IsEngineOn = _engineDisabled;
                Car.FuelLevel = _engineDisabled ? 0 : 100;
            }
        }

        /// <summary>
        /// Cleanup the scenario nicely so the player doesn't notice
        /// </summary>
        public void CleanupSafe()
        {
            if (PoliceDriver)
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

            if (TheifDriver)
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
            if (Car)
                Car.IsPersistent = false;

            // Remove our blip
            if (CarBlip)
                CarBlip.Delete();
        }

        /// <summary>
        /// Cleanup the plugin by deleting everything. Used when plugin is forced to unload
        /// </summary>
        public void Cleanup()
        {
            if (PoliceDriver)
                PoliceDriver.Delete();
            if (TheifDriver)
                TheifDriver.Delete();
            if (Car)
                Car.Delete();
            if (CarBlip)
                CarBlip.Delete();
        }
    }
}