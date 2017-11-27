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
        public event OnMenuItemSelectedEvent OnMenuItemSelected;
        public delegate void OnMenuItemSelectedEvent(MenuHandlerEventArgs e);

        #region MenuVariables
        private readonly UIMenu _mainMenu;
        private readonly UIMenu _carMenu;
        private readonly UIMenu _optionsMenu;
        private readonly UIMenu _optionsKeysMenu;
        private readonly UIMenu _optionsButtonsMenu;

        private readonly UIMenuListItem _requestCarVehicleSelector;
        private readonly UIMenuItem _requestCarCurrentVehicle;

        private readonly UIMenuItem _cancelCar;
        private readonly UIMenuItem _toggleEngine;

        private readonly UIMenuItem _optionKeyOpenMenu;
        private readonly UIMenuItem _optionKeyKillSwitch;

        private readonly UIMenuItem _optionButtonOpenMenu;
        private readonly UIMenuItem _optionButtonKillSwitch;

        private readonly UIMenuListItem _optionMinSecondsToWait;
        private readonly UIMenuListItem _optionMaxSecondsToWait;
        private readonly UIMenuListItem _optionMaxSearchRadius;
        private readonly UIMenuCheckboxItem _optionHardcore;
        private readonly UIMenuCheckboxItem _optionDebug;

        private readonly UIMenuItem _optionRevert;
        private readonly UIMenuItem _optionSave;
        #endregion

        public MenuHandler(ConfigHandler configHandler, StateHandler stateHandler)
        {
            LogHandler.Log("Initializing Menus...");
            _configHandler = configHandler;
            _stateHandler = stateHandler;

            #region MainMenu
            _mainMenu = new UIMenu(EntryPoint.Name, "Request a Car");
            _mainMenu.AddItem(_requestCarVehicleSelector = new UIMenuListItem("Select Car", "Select the car to spawn",
                new List<string> { "Random", "Carbonizzare", "Comet", "Baller", "Dominator" }));
            _mainMenu.AddItem(_requestCarCurrentVehicle = new UIMenuItem("Select Car You Are In", "Use the vehicle you are currently in as a bait car."));

            // TODO: Add ability to type car name
            // _mainMenu.AddItem(new UIMenuTextboxItem("Type Car Name", "Type the name of the vehicle you wish to spawn.", "TestContent"));
            #endregion

            #region CarMenu
            _carMenu = new UIMenu(EntryPoint.Name, "Car Controls");
            _carMenu.AddItem(_toggleEngine = new UIMenuItem("Toggle Engine"));
            _carMenu.AddItem(_cancelCar = new UIMenuItem("End Bait Car Session"));
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

            _optionsKeysMenu.AddItem(_optionKeyOpenMenu = new UIMenuItem("Open Main Menu", "The keybind to open this menu."));
            _optionsKeysMenu.AddItem(_optionKeyKillSwitch = new UIMenuItem("Kill Switch", "The keybind to shut off the bait car engine."));

            _optionsButtonsMenu.AddItem(_optionButtonOpenMenu = new UIMenuItem("Open Main Menu", "The button to open this menu."));
            _optionsButtonsMenu.AddItem(_optionButtonKillSwitch = new UIMenuItem("Kill Switch", "The button to shut off the bait car engine."));

            SetOptionsValues();
            #endregion

            _mainMenu.RefreshIndex();
            _carMenu.RefreshIndex();
            _optionsMenu.RefreshIndex();
            _optionsKeysMenu.RefreshIndex();
            _optionsButtonsMenu.RefreshIndex();
            _mainMenu.OnItemSelect += OnItemSelect;
            _carMenu.OnItemSelect += OnItemSelect;
            _carMenu.OnListChange += OnListChange;
            _optionsMenu.OnItemSelect += OnItemSelect;
            _optionsMenu.OnListChange += OnListChange;
            _optionsKeysMenu.OnItemSelect += OnItemSelect;
            _optionsButtonsMenu.OnItemSelect += OnItemSelect;
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
            _optionButtonOpenMenu.SetRightLabel(_configHandler.GetValue("Buttons", "OpenMenu", "None"));
            _optionButtonKillSwitch.SetRightLabel(_configHandler.GetValue("Buttons", "KillSwitch", "None"));
        }

        public void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == _mainMenu)
            {
                if (selectedItem == _requestCarVehicleSelector)
                {
                    _menuPool.CloseAllMenus();
                    _carMenu.Visible = true;
                    OnMenuItemSelected?.Invoke(new MenuHandlerEventArgs(MenuHandlerEventArgs.EventType.SpawnVehicle,
                        _requestCarVehicleSelector.SelectedItem.DisplayText));
                }
                else if (selectedItem == _requestCarCurrentVehicle && Game.LocalPlayer.Character.IsInAnyVehicle(true))
                {
                    OnMenuItemSelected?.Invoke(
                        new MenuHandlerEventArgs(MenuHandlerEventArgs.EventType.UseCurrentVehicle));
                    _menuPool.CloseAllMenus();
                    _carMenu.Visible = true;
                }
            }
            else if (sender == _carMenu)
            {
                if (selectedItem == _cancelCar)
                {
                    _menuPool.CloseAllMenus();
                    OnMenuItemSelected?.Invoke(new MenuHandlerEventArgs(MenuHandlerEventArgs.EventType.EndSession));
                }
                else if (selectedItem == _toggleEngine)
                {
                    OnMenuItemSelected?.Invoke(new MenuHandlerEventArgs(MenuHandlerEventArgs.EventType.ToggleEngine));
                }
            }
            else if (sender == _optionsMenu)
            {
                if (selectedItem == _optionSave)
                {
                    LogHandler.Log("Saving options...");

                    if (_configHandler.SetValues(new Dictionary<string, string>
                    {
                        { "Options.MinSecondsToWait", _optionMinSecondsToWait.SelectedValue.ToString() },
                        { "Options.MaxSecondsToWait", _optionMaxSecondsToWait.SelectedValue.ToString() },
                        { "Options.MaxSearchRadius", _optionMaxSearchRadius.SelectedValue.ToString() },
                        { "Options.Hardcore", _optionHardcore.Checked.ToString() },
                        { "Options.Debug", _optionDebug.Checked.ToString() },
                        //{ "Keys.OpenMenu", _optionKeyOpenMenu.RightLabel },
                        //{ "Keys.KillSwitch", _optionKeyKillSwitch.RightLabel },
                        //{ "Buttons.OpenMenu", _optionKeyOpenMenu.RightLabel },
                        //{ "Buttons.KillSwitch", _optionButtonKillSwitch.RightLabel }
                    }))
                    {
                        LogHandler.Log("Options saved!");
                    }
                }
                else if (selectedItem == _optionRevert)
                {
                    LogHandler.Log("Options reverted.", LogType.Debug);
                    SetOptionsValues();
                }
            }
            else if (sender == _optionsKeysMenu)
            {
                if (selectedItem == _optionKeyOpenMenu)
                {
                    // TODO: Set up rebinds
                }
                else if (selectedItem == _optionKeyKillSwitch)
                {
                    // TODO: Set up rebinds
                }
            }
            else if (sender == _optionsButtonsMenu)
            {
                if (selectedItem == _optionButtonOpenMenu)
                {
                    // TODO: Set up rebinds
                }
                else if (selectedItem == _optionButtonKillSwitch)
                {
                    // TODO: Set up rebinds
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
            var keys = _configHandler.GetKey("Keys", "OpenMenu", Keys.F7);
            var button = _configHandler.GetButton("Buttons", "OpenMenu");

            // Check if our keys are pressed to open the menu
            if (Game.IsKeyDown(keys.First()) && Game.IsKeyDown(keys.Last()) ||
                Game.IsControllerButtonDown(button))
            {
                if (_stateHandler.State == State.None)
                    _mainMenu.Visible = !_mainMenu.Visible;
                else
                    _carMenu.Visible = !_carMenu.Visible;
            }

            if (_mainMenu.Visible)
                _requestCarCurrentVehicle.Enabled = Game.LocalPlayer.Character.IsInAnyVehicle(true);

            _menuPool.ProcessMenus();
        }
    }

    public class MenuHandlerEventArgs : EventArgs
    {
        public enum EventType
        {
            SpawnVehicle,
            UseCurrentVehicle,
            ToggleEngine,
            EndSession
        }

        public EventType Type;
        public string VehicleType;

        public MenuHandlerEventArgs(EventType type)
        {
            Type = type;
        }

        public MenuHandlerEventArgs(EventType type, string vehicle)
        {
            Type = type;
            VehicleType = vehicle;
        }
    }
}
