using System;
using System.Linq;
using Exercise9.Menus;
using Exercise9.Services;
using Exercise9.Vehicles;

namespace Exercise9.UI
{
    internal static class FacilitiesMenu
    {
        public static Menu<GarageSummary> OpenFacility(IGarageHandler garageHandler)
        {
            return Menu<GarageSummary>.CreateSelectionMenu(
                () => "Available Facilities",
                garageHandler.GetGarages(),
                garage => $"{garage.Name} ({string.Join(", ", garage.VehicleTypes)})"
            );
        }

        public static Menu<GarageSummary> CreateFacility(IGarageHandler garageHandler)
        {
            return new Menu<GarageSummary>(
                () => "Create Facility",
                () =>
                [
                    new MenuItem<GarageSummary>(
                        "Create garage",
                        valueFactory: () => CreateGarage(garageHandler)
                    ),
                    new MenuItem<GarageSummary>("Back")
                ]
            );
        }

        private static GarageSummary? CreateGarage(IGarageHandler garageHandler)
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
