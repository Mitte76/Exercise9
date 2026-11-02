using System.Collections.Generic;
using System.Threading.Tasks;
using Exercise9.Menus;
using Exercise9.Services;

namespace Exercise9.UI
{
    internal class MainMenu()
    {
        public Task<GarageSummary?> Show(IGarageHandler garageHandler)
        {
            return new Menu<GarageSummary>(
                () => "Main Menu",
                () => GetMainMenuItems(garageHandler),
                isMainMenu: true
            ).ShowMenu();
        }

        private MenuItem<GarageSummary>[] GetMainMenuItems(IGarageHandler garageHandler)
        {
            List<MenuItem<GarageSummary>> items = [];

            if (garageHandler.GarageCount > 0)
            {
                items.Add(
                    new MenuItem<GarageSummary>(
                        "Open Facility",
                        submenu: FacilitiesMenu.OpenFacility(garageHandler)
                    )
                );
            }

            items.Add(
                new MenuItem<GarageSummary>(
                    "Add Facility",
                    submenu: FacilitiesMenu.CreateFacility(garageHandler)
                )
            );
            items.Add(new MenuItem<GarageSummary>("Exit"));
            return [.. items];
        }
    }
}
