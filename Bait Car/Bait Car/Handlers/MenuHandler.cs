using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace Bait_Car.Handlers
{
    public class MenuHandler
    {
        private readonly ConfigHandler _configHandler;
        private readonly StateHandler _stateHandler;
        
        private readonly MenuPool _menuPool;

        #region MenuVariables
        private UIMenu _mainMenu;
        private UIMenu _carMenu;
        private UIMenu _optionsMenu;
        private UIMenu _optionsKeysMenu;
        private UIMenu _optionsButtonsMenu;

        private UIMenuListItem _requestCarVehicleSelector;
        private UIMenuItem _requestCarCurrentVehicle;

        private UIMenuItem _cancelCar;
        private UIMenuItem _toggleEngine;
        private UIMenuItem _toggleLocks;

        private UIMenuItem _optionKeyOpenMenu;
        private UIMenuItem _optionKeyKillSwitch;
        private UIMenuItem _optionKeyLockDoors;

        private UIMenuItem _optionButtonOpenMenu;
        private UIMenuItem _optionButtonKillSwitch;
        private UIMenuItem _optionButtonLockDoors;

        private UIMenuListItem _optionMinSecondsToWait;
        private UIMenuListItem _optionMaxSecondsToWait;
        private UIMenuListItem _optionMaxSearchRadius;
        private UIMenuCheckboxItem _optionHardcore;
        private UIMenuCheckboxItem _optionDebug;

        private UIMenuItem _optionRevert;
        private UIMenuItem _optionSave;
        #endregion

        public MenuHandler(ConfigHandler configHandler, StateHandler stateHandler)
        {
            LogHandler.Log("Initializing Menus...");
            _configHandler = configHandler;
            _stateHandler = stateHandler;

            #region MainMenu
            _mainMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Request a Car");
            _mainMenu.AddItem(_requestCarVehicleSelector = new UIMenuListItem("Select Car", "Select the car to spawn",
                new List<string>{"Zentorno", "Carbonizzare", "Banshee", "Coquette", "Comet", "Elegy"}));
            _mainMenu.AddItem(_requestCarCurrentVehicle = new UIMenuItem("Select Car You Are In", "Use the vehicle you are currently in as a bait car."));

            _mainMenu.RefreshIndex();
            _mainMenu.OnItemSelect += OnItemSelect;
            #endregion

            #region CarMenu
            _carMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Car Controls");
            _carMenu.AddItem(_toggleEngine = new UIMenuItem("Toggle Engine"));
            _carMenu.AddItem(_toggleLocks = new UIMenuItem("Toggle Locks"));
            _carMenu.AddItem(_cancelCar = new UIMenuItem("End Bait Car Session"));

            _carMenu.RefreshIndex();
            _carMenu.OnItemSelect += OnItemSelect;
            _carMenu.OnListChange += OnListChange;
            #endregion

            _menuPool = new MenuPool {_mainMenu, _carMenu};

            #region OptionsMenu
            _optionsMenu = _menuPool.AddSubMenu(_mainMenu, "Options");

            _optionsMenu.AddItem(_optionMinSecondsToWait =
                new UIMenuListItem("Minimum Seconds to Wait",
                    "Minimum number of seconds to wait before someone steals the car.",
                    Enumerable.Range(10, 500).Where(w => w % 10 == 0).Select(s => s.ToString()).ToList()));

            _optionsMenu.AddItem(_optionMaxSecondsToWait =
                new UIMenuListItem("Maximum Seconds to Wait",
                    "Maximum number of seconds to wait before someone steals the car.",
                    Enumerable.Range(10, 500).Where(w => w % 10 == 0).Select(s => s.ToString()).ToList()));
            
            _optionsMenu.AddItem(_optionMaxSearchRadius =
                new UIMenuListItem("Maximum Search Radius",
                    "The radius to find a valid ped for stealing the car.",
                    Enumerable.Range(50, 200).Where(w => w % 10 == 0).Select(s => s.ToString()).ToList()));

            _optionsMenu.AddItem(_optionHardcore =
                new UIMenuCheckboxItem("Hardcore Mode",
                    configHandler.GetBoolean("Options", "Hardcore"),
                    "Disabled helpful tooltips if enabled."));

            _optionsMenu.AddItem(_optionDebug =
                new UIMenuCheckboxItem("Debug Mode",
                    configHandler.GetBoolean("Options", "Hardcore"),
                    "Disabled helpful tooltips if enabled."));

            _optionsKeysMenu = _menuPool.AddSubMenu(_optionsMenu, "Keys");
            _optionsButtonsMenu = _menuPool.AddSubMenu(_optionsMenu, "Buttons");

            _optionsMenu.AddItem(_optionRevert = new UIMenuItem("Revert Changes") { BackColor = Color.RoyalBlue });
            _optionsMenu.AddItem(_optionSave = new UIMenuItem("Save Changes") { BackColor = Color.RoyalBlue });
           
            _optionsMenu.RefreshIndex();
            _optionsMenu.OnItemSelect += OnItemSelect;
            _optionsMenu.OnListChange += OnListChange;

            _optionsKeysMenu.AddItem(_optionKeyOpenMenu = new UIMenuItem("Open Main Menu", "The keybind to open this menu."));
            _optionsKeysMenu.AddItem(_optionKeyKillSwitch = new UIMenuItem("Kill Switch", "The keybind to shut off the bait car engine."));
            _optionsKeysMenu.AddItem(_optionKeyLockDoors = new UIMenuItem("Lock Doors", "The keybind to lock the bait car doors."));
            _optionsKeysMenu.RefreshIndex();
            _optionsKeysMenu.OnItemSelect += OnItemSelect;

            _optionsButtonsMenu.AddItem(_optionButtonOpenMenu = new UIMenuItem("Open Main Menu", "The button to open this menu."));
            _optionsButtonsMenu.AddItem(_optionButtonKillSwitch = new UIMenuItem("Kill Switch", "The button to shut off the bait car engine."));
            _optionsButtonsMenu.AddItem(_optionButtonLockDoors = new UIMenuItem("Lock Doors", "The button to lock the bait car doors."));
            _optionsButtonsMenu.RefreshIndex();
            _optionsButtonsMenu.OnItemSelect += OnItemSelect;

            SetOptionsValues();
            #endregion

        }

        public void SetOptionsValues()
        {
            _optionMinSecondsToWait.Index = _configHandler.GetInteger("Options", "MinSecondsToWait", 30) / 10 - 1;
            _optionMaxSecondsToWait.Index = _configHandler.GetInteger("Options", "MaxSecondsToWait", 120) / 10 - 1;
            _optionMaxSearchRadius.Index = _configHandler.GetInteger("Options", "MaxSearchRadius", 100) / 10 - 1;
            _optionHardcore.Checked = _configHandler.GetBoolean("Options", "Hardcore");
            _optionDebug.Checked = _configHandler.GetBoolean("Options", "Debug");
            _optionKeyOpenMenu.SetRightLabel(_configHandler.GetValue("Keys", "OpenMenu", "F7"));
            _optionKeyKillSwitch.SetRightLabel(_configHandler.GetValue("Keys", "KillSwitch", "K"));
            _optionKeyLockDoors.SetRightLabel(_configHandler.GetValue("Keys", "LockDoors", "L"));
            _optionButtonOpenMenu.SetRightLabel(_configHandler.GetValue("Buttons", "OpenMenu", "None"));
            _optionButtonKillSwitch.SetRightLabel(_configHandler.GetValue("Buttons", "KillSwitch", "None"));
            _optionButtonLockDoors.SetRightLabel(_configHandler.GetValue("Buttons", "LockDoors", "None"));
        }

        public void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            LogHandler.Log("Sender: " + sender.Subtitle, LogType.DEBUG);
            LogHandler.Log("selectedItem: " + selectedItem.Text, LogType.DEBUG);

            if (sender == _mainMenu)
            {
                if (selectedItem == _requestCarVehicleSelector)
                {
                    LogHandler.Log(_requestCarVehicleSelector.SelectedItem.DisplayText + " Selected", LogType.DEBUG);
                    _stateHandler.State = State.DrivingToPlayer;
                    _menuPool.CloseAllMenus();
                    _carMenu.Visible = !_carMenu.Visible;
                }
                else if (selectedItem == _requestCarCurrentVehicle && Game.LocalPlayer.Character.IsInAnyVehicle(true))
                {
                    LogHandler.Log(Game.LocalPlayer.Character.CurrentVehicle.Model.Name + " Selected", LogType.DEBUG);
                    _stateHandler.State = State.PlayerParking;
                    _menuPool.CloseAllMenus();
                    _carMenu.Visible = !_carMenu.Visible;
                }
            }
            else if (sender == _carMenu)
            {
                if (selectedItem == _cancelCar)
                {
                    LogHandler.Log("Ending bait car session");
                    // TODO: End session
                    _stateHandler.State = State.None;
                    _menuPool.CloseAllMenus();
                }
                else if (selectedItem == _toggleEngine)
                {
                    // TODO: Shut off bait car engine
                }
                else if (selectedItem == _toggleLocks)
                {
                    // TODO: Lock bait car doors
                }
            }
            else if (sender == _optionsMenu)
            {
                if (selectedItem == _optionSave)
                {
                    LogHandler.Log("Saving options...");
                    // TODO: Save changes
                }
                else if (selectedItem == _optionRevert)
                {
                    LogHandler.Log("Options reverted.", LogType.DEBUG);
                    SetOptionsValues();
                }
            }
            else if (sender == _optionsKeysMenu)
            {
                if (selectedItem == _optionKeyOpenMenu)
                {

                }
                else if (selectedItem == _optionKeyKillSwitch)
                {

                }
                else if (selectedItem == _optionKeyLockDoors)
                {

                }
            }
            else if (sender == _optionsButtonsMenu)
            {
                if (selectedItem == _optionButtonOpenMenu)
                {

                }
                else if (selectedItem == _optionButtonKillSwitch)
                {

                }
                else if (selectedItem == _optionButtonLockDoors)
                {

                }
            }
        }

        public void OnListChange(UIMenu sender, UIMenuListItem list, int index)
        {
            if (list == _optionMinSecondsToWait)
            {
                if (int.Parse((string) list.SelectedValue) == int.Parse((string) _optionMaxSecondsToWait.SelectedValue))
                {
                    list.Index--;
                }
            }

            if (list == _optionMaxSecondsToWait)
            {
                if (int.Parse((string)list.SelectedValue) == int.Parse((string)_optionMinSecondsToWait.SelectedValue))
                {
                    list.Index++;
                }
            }
        }

        public void Update()
        {
            if (Game.IsKeyDown(_configHandler.GetKey("Keys", "OpenMenu", Keys.F7)) && !_menuPool.IsAnyMenuOpen())
            {
                switch (_stateHandler.State)
                {
                    case State.None:
                        _mainMenu.Visible = !_mainMenu.Visible;
                        break;
                    default:
                        _carMenu.Visible = !_carMenu.Visible;
                        break;
                }
            }

            if (_mainMenu.Visible)
            {
                _requestCarCurrentVehicle.Enabled = Game.LocalPlayer.Character.IsInAnyVehicle(true);
            }

            _menuPool.ProcessMenus();
        }
    }
}
