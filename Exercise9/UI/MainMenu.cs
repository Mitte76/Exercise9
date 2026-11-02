using System.Collections.Generic;
using Exercise9.Menus;
using Exercise9.Services;

namespace Exercise9.UI
{
    internal class MainMenu(GarageHandler garageHandler)
    {
        public Menu Show()
        {
            return new Menu(() => "Main Menu", GetMainMenuItems, isMainMenu: true);
        }

        private MenuItem[] GetMainMenuItems()
        {
            var list = new List<MenuItem>();

            if (_facilities.Count > 0)
            {
                list.Add(new MenuItem("Open Facilitity", submenu: OpenFacility()));
            }

            list.Add(new MenuItem("Add Facility", submenu: CreateFacilityMenu()));

            list.Add(
                new MenuItem(
                    "Load/Save Data",
                    submenu: new Menu(() => "Main Menu", GetDatabaseOptions)
                )
            );

            list.Add(new MenuItem("Exit"));
            return [.. list];
        }
    }
}
