using Exercise9.Facilities;
using Exercise9.Menus;
using Exercise9.Services;
using Exercise9.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exercise9.UI
{
    internal class MainMenu()
    {
        public Task<Garage<Vehicle>?> ShowAsync(GarageHandler garageHandler)
        {
            return new Menu<Garage<Vehicle>>(
                () => "Main Menu",
                () => GetMainMenuItems(garageHandler),
                isMainMenu: true
            ).ShowMenu();
        }

        private MenuItem<Garage<Vehicle>>[] GetMainMenuItems(GarageHandler garageHandler)
        {
            var list = new List<MenuItem<Garage<Vehicle>>>();

            if (garageHandler.GarageCount() > 0)
            {
                list.Add(
                    new MenuItem<Garage<Vehicle>>(
                        "Open Facility",
                        submenu: FacilityMenu.OpenFacility(garageHandler)
                    )
                );
            }

            list.Add(
                new MenuItem<Garage<Vehicle>>(
                    "Add Facility",
                    submenu: FacilityMenu.CreateFacility(garageHandler)
                )
            );
            list.Add(new MenuItem<Garage<Vehicle>>("Exit"));
            return [.. list];
        }
    }
}
