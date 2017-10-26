using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace Bait_Car.Handlers
{
    public class MenuHandler
    {
        private ConfigHandler _configHandler;
        private StateHandler _stateHandler;
        private bool _inOptions;
        
        private MenuPool _menuPool;

        private UIMenu _mainMenu;
        private UIMenu _carMenu;
        private UIMenu _optionsMenu;
        
        private UIMenuListItem _requestCarVehicleSelector;
        private UIMenuItem _requestCarCurrentVehicle;

        private UIMenuItem _cancelCar;
        private UIMenuItem _toggleEngine;
        private UIMenuItem _toggleLocks;

        public MenuHandler(ConfigHandler configHandler, StateHandler stateHandler)
        {
            LogHandler.Log("Initializing Menu...", LogType.DEBUG);
            _configHandler = configHandler;
            _stateHandler = stateHandler;

            _menuPool = new MenuPool();

            _mainMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Request a Car");
            _carMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Car Controls");
            _optionsMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Options");

            _mainMenu.AddItem(_requestCarVehicleSelector = new UIMenuListItem("Select Car", "Select the car to spawn",
                new List<string>{"Zentorno", "Carbonizzare", "Banshee", "Coquette", "Comet", "Elegy"}));
            _mainMenu.AddItem(_requestCarCurrentVehicle = new UIMenuItem("Select Car You Are In", "Use the vehicle you are currently in as a bait car."));

            _carMenu.AddItem(_toggleEngine = new UIMenuItem("Toggle Engine"));
            _carMenu.AddItem(_toggleLocks = new UIMenuItem("Toggle Locks"));
            _carMenu.AddItem(_cancelCar = new UIMenuItem("End Bait Car Session"));

            // TODO: Implement options menu

            _mainMenu.RefreshIndex();
            _mainMenu.OnItemSelect += OnItemSelect;
            _mainMenu.OnIndexChange += OnItemChange;

            _carMenu.RefreshIndex();
            _carMenu.OnItemSelect += OnItemSelect;
            _carMenu.OnListChange += OnListChange;
            _carMenu.OnIndexChange += OnItemChange;

            _optionsMenu.RefreshIndex();
            _optionsMenu.OnItemSelect += OnItemSelect;
            _optionsMenu.OnListChange += OnListChange;
            _optionsMenu.OnIndexChange += OnItemChange;
            _optionsMenu.OnCheckboxChange += OnCheckboxChange;

            _menuPool.Add(_mainMenu);
            _menuPool.Add(_carMenu);
        }

        public void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == _requestCarVehicleSelector)
            {
                LogHandler.Log(_requestCarVehicleSelector.SelectedItem.DisplayText + " Selected");
                _stateHandler.State = State.DrivingToPlayer;
            }
            else if (selectedItem == _requestCarCurrentVehicle && Game.LocalPlayer.Character.IsInAnyVehicle(true))
            {
                LogHandler.Log(Game.LocalPlayer.Character.CurrentVehicle.Model.Name + " Selected");
                _stateHandler.State = State.PlayerParking;
            }
        }

        public void OnItemChange(UIMenu sender, int index)
        {
            
        }

        public void OnListChange(UIMenu sender, UIMenuListItem list, int index)
        {
            
        }

        public void OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkbox, bool Checked)
        { 

        }

        public void Update()
        {
            if (Game.IsKeyDown(_configHandler.GetKey("Keys", "OpenMenu", Keys.F7)) && !_menuPool.IsAnyMenuOpen())
            {
                switch (_stateHandler.State)
                {
                    case State.None:
                        if (!_inOptions)
                            _mainMenu.Visible = !_mainMenu.Visible;
                        else
                            _optionsMenu.Visible = !_optionsMenu.Visible;
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
