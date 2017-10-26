using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bait_Car.Handlers;

namespace Bait_Car
{
    using Rage;
    using RAGENativeUI;
    using RAGENativeUI.Elements;

    public class Menu
    {
        private ConfigHandler _configHandler;
        private GameFiber _menusProcessFiber;
        private UIMenu _mainMenu;
        private UIMenu _carMenu;
        private UIMenu _optionsMenu;
        
        private UIMenuListItem _requestCarVehicleSelector;
        private UIMenuItem _requestCarCurrentVehicle;

        private UIMenuItem _cancelCar;
        private UIMenuItem _toggleEngine;
        private UIMenuItem _toggleLocks;

        private MenuPool _menuPool;

        public Menu(ConfigHandler configHandler)
        {
            _configHandler = configHandler;
            _menusProcessFiber = new GameFiber(ProcessLoop);

            _menuPool = new MenuPool();

            _mainMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Request a Car");
            _carMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Car Controls");
            _optionsMenu = new UIMenu($"Bait Car {EntryPoint.Version}", "Options");

            _mainMenu.AddItem(_requestCarVehicleSelector = new UIMenuListItem("Car Selector", "Select the car to spawn",
                new List<string>{"Zentorno", "Carbonizzare", "Banshee", "Coquette", "Comet", "Elegy"}));
            _mainMenu.AddItem(_requestCarCurrentVehicle = new UIMenuItem("Select Current Vehicle"));

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
            _menuPool.Add(_optionsMenu);
        }

        public void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            
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

        public void ProcessLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                if (Game.IsKeyDown(_configHandler.GetKey("Keys", "OpenMenu", Keys.F7)) && !_menuPool.IsAnyMenuOpen())
                {
                    _mainMenu.Visible = !_mainMenu.Visible;
                }

                _menuPool.ProcessMenus();
            }
        }
    }
}
