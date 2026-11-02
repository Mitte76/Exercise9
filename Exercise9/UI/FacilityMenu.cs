using System;
using System.Linq;
using Exercise9.Facilities;
using Exercise9.Menus;
using Exercise9.Services;
using Exercise9.Vehicles;

namespace Exercise9.UI
{
    internal static class FacilityMenu
    {
        public static Menu<Garage<Vehicle>> OpenFacility(GarageHandler garageHandler)
        {
            return Menu<Garage<Vehicle>>.CreateSelectionMenu(
                () => "Available Facilities",
                garageHandler.GetGarages(),
                garage => $"{garage.Name} ({garage.GetAllowedVehicles()})"
            );
        }
        public static Menu<Garage<Vehicle>> CreateFacility(GarageHandler garageHandler)
        {
            return new Menu<Garage<Vehicle>>(
                () => "Create Facility",
                () =>
                [
                    new MenuItem<Garage<Vehicle>>(
                        "Create garage",
                        valueFactory: () => CreateGarage(garageHandler)
                    ),
                    new MenuItem<Garage<Vehicle>>("Back")
                ]
            );
        }

        private static Garage<Vehicle>? CreateGarage(GarageHandler garageHandler)
        {
            Console.Write("Enter garage name (leave empty to cancel): ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            while (true)
            {
                Console.Write("Enter number of parking spaces (leave empty to cancel): ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (!int.TryParse(input, out int capacity) || capacity <= 0)
                {
                    Console.WriteLine("Capacity must be a positive whole number.");
                    continue;
                }

                VehicleType[]? vehicleTypes = SelectVehicleTypes();
                if (vehicleTypes == null)
                    return null;

                return garageHandler.AddGarage(name.Trim(), capacity, vehicleTypes);
            }
        }

        private static VehicleType[]? SelectVehicleTypes()
        {
            VehicleType[] vehicleTypes = Enum.GetValues<VehicleType>();
            bool[] selected = new bool[vehicleTypes.Length];
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select vehicle types (toggle with number or Space):");
                Console.WriteLine("Use Up/Down to navigate, Enter to confirm, Esc to cancel.\n");

                for (int i = 0; i < vehicleTypes.Length; i++)
                {
                    string checkbox = selected[i] ? "[x]" : "[ ]";
                    string marker = i == selectedIndex ? ">" : " ";
                    Console.WriteLine($"{marker} {i + 1}. {checkbox} {vehicleTypes[i]}");
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + vehicleTypes.Length) % vehicleTypes.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % vehicleTypes.Length;
                        break;
                    case ConsoleKey.Spacebar:
                        selected[selectedIndex] = !selected[selectedIndex];
                        break;
                    case ConsoleKey.Enter:
                        VehicleType[] chosenTypes = vehicleTypes
                            .Where((_, index) => selected[index])
                            .ToArray();

                        if (chosenTypes.Length > 0)
                            return chosenTypes;

                        Console.WriteLine("Select at least one vehicle type.");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.Escape:
                        return null;
                    default:
                        if (char.IsDigit(key.KeyChar))
                        {
                            int option = key.KeyChar - '0';
                            if (option > 0 && option <= vehicleTypes.Length)
                            {
                                selectedIndex = option - 1;
                                selected[selectedIndex] = !selected[selectedIndex];
                            }
                        }
                        break;
                }
            }
        }
    }
}
