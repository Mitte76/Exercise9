using System.Linq;
using System.Threading.Tasks;
using Exercise9.Facilities;
using Exercise9.Menus;
using Exercise9.Vehicles;

namespace Exercise9.UI
{
    internal static class GarageMenu
    {
        public static Task<string?> ShowAsync(Garage<Vehicle> garage)
        {
            return new Menu<string>(
                () => $"Garage: {garage.Name}",
                () => GetMenuItems(garage),
                extra: () => $"Allowed vehicles: {string.Join(", ", garage.VehicleTypes)}"
            ).ShowMenu();
        }

        private static MenuItem<string>[] GetMenuItems(Garage<Vehicle> garage)
        {
            return
            [
                new MenuItem<string>(
                    "Show vehicle count",
                    action: () => $"Number of vehicles: {garage.Count()}"
                ),
                new MenuItem<string>("Back")
            ];
        }
    }
}
