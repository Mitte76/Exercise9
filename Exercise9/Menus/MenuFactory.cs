using System;
using System.Collections.Generic;
using System.Text;
using Exercise6.Data;
using Exercise6.Facilities;
using Exercise6.Services;
using Exercise6.Vehicles;

namespace Exercise6.Menus
{
    internal class MenuFactory(List<Garage> facilities)
    {
        private readonly List<Garage> _facilities = facilities ?? [];

        public Menu CreateMainMenu()
        {
            return new Menu(() => "Main Menu", GetMainMenuItems, isMainMenu: true);
        }

        private Menu OpenFacility()
        {
            return new Menu(() => "Available Facilities", GetAvailableFacilities);
        }

        private MenuItem[] GetAvailableFacilities()
        {
            var list = new List<MenuItem>();

            foreach (var facility in _facilities)
            {
                list.Add(
                    new MenuItem(
                        $"{facility.Name} ({facility.GetType().Name}) (Available: {facility.AvailableSlots}/{facility.Capacity})",
                        submenu: FacilityMenu(facility)
                    )
                );
            }

            list.Add(new MenuItem("Back"));

            return [.. list];
        }

        private Menu FacilityMenu(Garage garage)
        {
            return new Menu(
                () => $"{garage.GetType().Name}: {garage.Name}",
                () => GarageSubMenus(garage)
            );
        }

        private MenuItem[] GarageSubMenus(Garage garage)
        {
            string vehicleTypes = string.Join("/", garage.VehicleTypes);
            MenuItem parkVehicle =
                garage.VehicleTypes.Length == 1
                    ? new MenuItem(
                        $"Park {vehicleTypes}",
                        action: () => ParkVehicle(garage, garage.VehicleTypes[0])
                    )
                    : new MenuItem($"Park {vehicleTypes}", submenu: ParkVehicle(garage));

            return
            [
                new MenuItem(
                    "Show vehicle type count",
                    action: () => PrintVehicleTypeCounts(garage)
                ),
                new MenuItem($"Find {vehicleTypes}", submenu: GetVehicle(garage)),
                parkVehicle,
                new MenuItem(
                    "Populate garage with random vehicles",
                    action: () => GaragePopulationService.PopulateGarage(garage, _facilities)
                ),
                new MenuItem("Back"),
            ];
        }

        private static string PrintVehicleTypeCounts(Garage garage)
        {
            StringBuilder result = new();

            foreach (var count in garage.GetVehicleTypeCounts())
            {
                result.AppendLine($"{count.Key}: {count.Value}");
            }

            return result.ToString().TrimEnd();
        }

        private Menu GetVehicle(Garage garage)
        {
            return new Menu(
                () => "Find Vehicle",
                () =>
                {
                    var vehicles = new List<MenuItem>()
                    {
                        new MenuItem("List all vehicles", submenu: GetAllVehicles(garage)),
                        new MenuItem("Search for vehicle", submenu: SearchVehicleMenu(garage)),
                    };

                    return [.. vehicles, new MenuItem("Back")];
                }
            );
        }

        private Menu SearchVehicleMenu(Garage garage)
        {
            return new Menu(
                () => "Search Vehicle",
                () =>
                {
                    var vehicles = new List<MenuItem>()
                    {
                        new MenuItem(
                            "Search by Licence plate",
                            submenuFactory: () => SearchByPlate(garage)
                        ),
                        new MenuItem(
                            "Search by multiple chriteria",
                            submenuFactory: () => SearchByCriteria(garage)
                        ),
                    };

                    return [.. vehicles, new MenuItem("Back")];
                }
            );
        }

        private Menu? SearchByPlate(Garage garage)
        {
            Console.WriteLine("Enter license plate. Leave empty to cancel.");

            string licensePlate = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(licensePlate))
                return null;

            try
            {
                licensePlate = Vehicle.ValidateLicensePlateNumber(licensePlate);
            }
            catch (ArgumentException exception)
            {
                return new Menu(
                    () => "Invalid License Plate",
                    () => [new MenuItem("Back")],
                    extra: () => exception.Message
                );
            }

            return new Menu(() => "Found Vehicles", () => SearchResults(garage, licensePlate));
        }

        private MenuItem[] SearchResults(Garage garage, string licensePlate)
        {
            var vehicles = new List<MenuItem>();

            foreach (ParkingSpot spot in garage.ParkingSpots)
            {
                if (spot.Vehicle is not Vehicle vehicle)
                    continue;

                bool isMatch = vehicle switch
                {
                    Car car => garage.VehicleTypes.Contains(VehicleType.Car)
                        && car.LicensePlateNumber.Equals(
                            licensePlate,
                            StringComparison.OrdinalIgnoreCase
                        ),
                    Bus bus => garage.VehicleTypes.Contains(VehicleType.Bus)
                        && bus.LicensePlateNumber.Equals(
                            licensePlate,
                            StringComparison.OrdinalIgnoreCase
                        ),
                    Motorcycle motorcycle => garage.VehicleTypes.Contains(VehicleType.Motorcycle)
                        && motorcycle.LicensePlateNumber.Equals(
                            licensePlate,
                            StringComparison.OrdinalIgnoreCase
                        ),
                    _ => false,
                };

                if (!isMatch)
                    continue;

                vehicles.Add(
                    new MenuItem(
                        $"{vehicle.Brand} ({vehicle.GetType().Name})",
                        submenu: EditVehicle(spot)
                    )
                );
            }

            vehicles.Add(new MenuItem("Back"));

            return [.. vehicles];
        }

        private Menu SearchByCriteria(Garage garage)
        {
            Dictionary<string, string> criterias = new(StringComparer.OrdinalIgnoreCase);

            List<string> availableCriteria =
            [
                "Brand",
                "Color",
                "LicensePlateNumber",
                "FuelType",
                "CylinderVolume",
                "Length",
                "NumberOfEngines",
            ];
            string input = "";
            while (true)
            {
                Console.Clear();
                Console.WriteLine(
                    """
                    Enter criteria as Criterion: Value.
                    Leave an empty line to start search.
                    """
                );

                if (criterias.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nSelected criterias:");
                    foreach (var criteria in criterias)
                    {
                        Console.WriteLine($"{criteria.Key}: {criteria.Value}");
                    }
                    Console.ResetColor();
                }

                string criterion = input.Split(':')[0].Trim();

                List<string> suggestions = [];

                foreach (string c in availableCriteria)
                {
                    if (c.StartsWith(criterion, StringComparison.OrdinalIgnoreCase))
                        suggestions.Add(c);
                }

                Console.WriteLine("\nAvailable criteria:");

                foreach (string suggestion in suggestions)
                {
                    bool isSelected = input.StartsWith(
                        suggestion + ": ",
                        StringComparison.OrdinalIgnoreCase
                    );

                    Console.ForegroundColor = isSelected ? ConsoleColor.Green : ConsoleColor.Gray;

                    Console.Write(suggestion + " ");
                }

                Console.WriteLine();

                Console.ResetColor();

                Console.Write(input);
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input[..^1];
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    if (input.Length == 0)
                        break;

                    //TODO Working here!!!
                    if (suggestions.Count == 1)
                    {
                        input = $"{suggestions[0]}: ";
                    }
                    else if (suggestions.Count > 2)
                    {
                        if (input == $"{suggestions[0]}: ")
                        {
                            input = suggestions[1];
                        }
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (input.Length == 0)
                        break;

                    string[] parts = input.Split(':', 2);

                    if (
                        parts.Length == 2
                        && !string.IsNullOrWhiteSpace(parts[0])
                        && !string.IsNullOrWhiteSpace(parts[1])
                    )
                    {
                        string enteredCriterion = parts[0].Trim();
                        string? criterionName = null;

                        foreach (string c in availableCriteria)
                        {
                            if (c.Equals(enteredCriterion, StringComparison.OrdinalIgnoreCase))
                            {
                                criterionName = c;
                                break;
                            }
                        }

                        if (criterionName != null)
                        {
                            string value = parts[1].Trim();
                            if (criterias.TryGetValue(criterionName, out string? currentValue))
                                criterias[criterionName] = $"{currentValue},{value}";
                            else
                                criterias.Add(criterionName, value);

                            input = "";
                        }
                    }
                }
            }

            StringBuilder extras = new StringBuilder();
            foreach (var item in criterias)
            {
                extras.AppendLine($"{item.Key}: {item.Value}");
            }

            return new Menu(
                () => "Search Results",
                () => SearchByCriteriaResults(garage, criterias),
                extra: () => extras.ToString()
            );
        }

        private MenuItem[] SearchByCriteriaResults(
            Garage garage,
            Dictionary<string, string> criteria
        )
        {
            var results = new List<MenuItem>();

            foreach (ParkingSpot spot in garage.ParkingSpots)
            {
                if (spot.Vehicle is not Vehicle vehicle)
                    continue;

                bool matches = true;

                foreach (var criterion in criteria)
                {
                    string? value = criterion.Key switch
                    {
                        "Brand" => vehicle.Brand,
                        "Color" => vehicle.Color,
                        "LicensePlateNumber" => vehicle switch
                        {
                            Car car => car.LicensePlateNumber,
                            Bus bus => bus.LicensePlateNumber,
                            Motorcycle motorcycle => motorcycle.LicensePlateNumber,
                            _ => null,
                        },
                        "FuelType" => vehicle is Car car ? car.FuelType.ToString() : null,
                        "CylinderVolume" => vehicle is Motorcycle motorcycle
                            ? motorcycle.CylinderVolume.ToString()
                            : null,
                        "Length" => vehicle is Boat boat ? boat.Length.ToString() : null,
                        "NumberOfEngines" => vehicle is Airplane airplane
                            ? airplane.NumberOfEngines.ToString()
                            : null,
                        _ => null,
                    };

                    bool criterionMatches = false;

                    if (value != null)
                    {
                        foreach (string searchValue in criterion.Value.Split(','))
                        {
                            if (
                                value.Contains(
                                    searchValue.Trim(),
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            {
                                criterionMatches = true;
                                break;
                            }
                        }
                    }

                    if (!criterionMatches)
                    {
                        matches = false;
                        break;
                    }
                }

                if (!matches)
                    continue;

                results.Add(
                    new MenuItem(
                        $"{vehicle.Brand} ({vehicle.GetType().Name})",
                        submenu: EditVehicle(spot)
                    )
                );
            }

            if (results.Count == 0)
                results.Add(new MenuItem("No matching vehicles found"));

            results.Add(new MenuItem("Back"));

            return [.. results];
        }

        private Menu GetAllVehicles(Garage garage)
        {
            int currentPage = 0;
            int totalPages = 1;
            const int pageSize = 20;

            return new Menu(
                () => "Select Vehicle",
                () =>
                {
                    var allVehicles = garage.GetAllOccupiesSpots();

                    totalPages = Math.Max(
                        1,
                        (int)Math.Ceiling(allVehicles.Count / (double)pageSize)
                    );
                    currentPage = Math.Clamp(currentPage, 0, totalPages - 1);

                    int startIndex = currentPage * pageSize;
                    int range = Math.Min(pageSize, allVehicles.Count - startIndex);

                    var items2 = allVehicles.GetRange(startIndex, range);
                    List<MenuItem> items = [];
                    foreach (ParkingSpot item in items2)
                    {
                        items.Add(
                            new MenuItem(
                                $"{item.Vehicle!.Brand} ({item.Vehicle.GetType().Name}) parkingSpot: {item.Id}",
                                submenu: EditVehicle(item)
                            )
                        );
                    }

                    if (currentPage > 0)
                        items.Add(
                            new MenuItem(
                                "Previous page",
                                action: () =>
                                {
                                    currentPage--;
                                    return null;
                                }
                            )
                        );

                    if (currentPage < totalPages - 1)
                        items.Add(
                            new MenuItem(
                                "Next page",
                                action: () =>
                                {
                                    currentPage++;
                                    return null;
                                }
                            )
                        );

                    items.Add(new MenuItem("Back"));

                    return [.. items];
                },
                extra: () => $"Page {currentPage + 1} of {totalPages}"
            );
        }

        private Menu ParkVehicle(Garage garage)
        {
            return new Menu(
                () => "Select vehicle type",
                () =>
                {
                    List<MenuItem> items = [];

                    foreach (var type in garage.VehicleTypes)
                    {
                        items.Add(
                            new MenuItem(type.ToString(), action: () => ParkVehicle(garage, type))
                        );
                    }

                    items.Add(new MenuItem("Back"));

                    return [.. items];
                }
            );
        }

        private string? ParkVehicle(Garage garage, VehicleType vehicleType)
        {
            try
            {
                Vehicle? vehicle = GetNewVehicle(vehicleType);

                if (vehicle == null)
                    return "Parking aborted";

                return garage.ParkCar(vehicle);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private Vehicle? GetNewVehicle(VehicleType vehicleType)
        {
            switch (vehicleType)
            {
                case VehicleType.Airplane:
                    return Airplane.Create();
                case VehicleType.Boat:
                    return Boat.Create();
                case VehicleType.Bus:
                    return Bus.Create(_facilities);
                case VehicleType.Car:
                    return Car.Create(_facilities);
                case VehicleType.Motorcycle:
                    return Motorcycle.Create(_facilities);
                default:
                    return null;
            }
        }

        private Menu EditVehicle(ParkingSpot parkingSpot)
        {
            return new Menu(
                () => GetVehicleHeader(parkingSpot.Vehicle!),
                () => EditVehicleSubMenu(parkingSpot),
                extra: () => GetStats(parkingSpot)
            );
        }

        private static string GetStats(ParkingSpot parkingSpot)
        {
            StringBuilder sb = new StringBuilder();
            switch (parkingSpot.Vehicle)
            {
                case Car car:
                    sb.AppendLine($"Brand: {car.Brand}");
                    sb.AppendLine($"License plate: {car.LicensePlateNumber}");
                    sb.AppendLine($"Fuel type: {car.FuelType}");
                    sb.AppendLine($"Color: {car.Color}");
                    sb.Append($"ParkingSpot nr: {parkingSpot.Id}");
                    break;

                case Bus bus:
                    sb.AppendLine($"Brand: {bus.Brand}");
                    sb.AppendLine($"License plate: {bus.LicensePlateNumber}");
                    sb.AppendLine($"Color: {bus.Color}");
                    sb.AppendLine($"Seats: {bus.NumberOfSeats}");
                    sb.Append($"ParkingSpot nr: {parkingSpot.Id}");
                    break;

                case Motorcycle motorcycle:
                    sb.AppendLine($"Brand: {motorcycle.Brand}");
                    sb.AppendLine($"License plate: {motorcycle.LicensePlateNumber}");
                    sb.AppendLine($"Engine volume: {motorcycle.CylinderVolume}cc");
                    sb.AppendLine($"Color: {motorcycle.Color}");
                    sb.Append($"ParkingSpot nr: {parkingSpot.Id}");
                    break;

                case Boat boat:
                    sb.AppendLine($"Brand: {boat.Brand}");
                    sb.AppendLine($"Length: {boat.Length}m");
                    sb.AppendLine($"Color: {boat.Color}");
                    sb.Append($"ParkingSpot nr: {parkingSpot.Id}");
                    break;

                case Airplane airplane:
                    sb.AppendLine($"Brand: {airplane.Brand}");
                    sb.AppendLine($"Number of engines: {airplane.NumberOfEngines}");
                    sb.AppendLine($"Color: {airplane.Color}");
                    sb.Append($"ParkingSpot nr: {parkingSpot.Id}");
                    break;
            }

            return sb.ToString();
        }

        private static string GetVehicleHeader(Vehicle vehicle)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{vehicle.Brand} ");

            switch (vehicle)
            {
                case Car car:
                    sb.Append($"{car.LicensePlateNumber} ");
                    break;
                case Motorcycle motorcycle:
                    sb.Append($"{motorcycle.LicensePlateNumber} ");
                    break;
                case Bus bus:
                    sb.Append($"{bus.LicensePlateNumber} ");
                    break;
                case Boat boat:
                    sb.Append($"{boat.Length} ");
                    break;
                case Airplane airplane:
                    sb.Append($"{airplane.NumberOfEngines} ");
                    break;
            }

            return sb.ToString().TrimEnd();
        }

        private MenuItem[] EditVehicleSubMenu(ParkingSpot spot)
        {
            return
            [
                new MenuItem(
                    "Remove vehicle",
                    action: () =>
                    {
                        spot.RemoveVehicle();
                        return "Vehicle removed";
                    },
                    backUpWhenDone: true
                ),
                new MenuItem(
                    "Change Brand",
                    action: () =>
                        EditVehicleProperty(value => spot.Vehicle!.Brand = value, "brand name")
                ),
                new MenuItem(
                    "Change Color",
                    action: () => EditVehicleProperty(value => spot.Vehicle!.Color = value, "color")
                ),
                .. GetVehicleSpecificMenuItems(spot.Vehicle!),
                new MenuItem("Back"),
            ];
        }

        private List<MenuItem> GetVehicleSpecificMenuItems(Vehicle vehicle)
        {
            return vehicle switch
            {
                Car car =>
                [
                    new MenuItem(
                        "Change License Plate Number",
                        action: () =>
                            EditVehicleProperty(
                                value =>
                                    car.LicensePlateNumber = Vehicle.ValidateLicensePlateNumber(
                                        value,
                                        _facilities,
                                        car
                                    ),
                                "license plate number"
                            )
                    ),
                ],
                Bus bus =>
                [
                    new MenuItem(
                        "Change License Plate Number",
                        action: () =>
                            EditVehicleProperty(
                                value =>
                                    bus.LicensePlateNumber = Vehicle.ValidateLicensePlateNumber(
                                        value,
                                        _facilities,
                                        bus
                                    ),
                                "license plate number"
                            )
                    ),
                    new MenuItem(
                        "Change number of seats",
                        action: () =>
                            EditVehicleProperty(
                                value => bus.NumberOfSeats = value,
                                "number of seats"
                            )
                    ),
                ],
                Motorcycle motorcycle =>
                [
                    new MenuItem(
                        "Change License Plate Number",
                        action: () =>
                            EditVehicleProperty(
                                value =>
                                    motorcycle.LicensePlateNumber =
                                        Vehicle.ValidateLicensePlateNumber(
                                            value,
                                            _facilities,
                                            motorcycle
                                        ),
                                "license plate number"
                            )
                    ),
                    new MenuItem(
                        "Change engine volume",
                        action: () =>
                            EditVehicleProperty(
                                value => motorcycle.CylinderVolume = value,
                                "engine volume"
                            )
                    ),
                ],

                Boat boat =>
                [
                    new MenuItem(
                        "Change Length",
                        action: () => EditVehicleProperty(value => boat.Length = value, "Length")
                    ),
                ],

                Airplane airplane =>
                [
                    new MenuItem(
                        "Change number of engines",
                        action: () =>
                            EditVehicleProperty(
                                value => airplane.NumberOfEngines = value,
                                "number of engines"
                            )
                    ),
                ],

                _ => [],
            };
        }

        private static string? EditVehicleProperty(Action<int> update, string propertyName)
        {
            Console.WriteLine($"Enter new {propertyName}. Leave empty to exit");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return $"Changing {propertyName} aborted";

            if (!int.TryParse(input, out int value))
                return $"Invalid {propertyName}. Please enter a number.";

            try
            {
                update(value);
                return $"{propertyName} updated successfully";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static string? EditVehicleProperty(Action<string> update, string propertyName)
        {
            Console.WriteLine($"Enter new {propertyName}. Leave empty to exit");
            string? value = Console.ReadLine();

            if (string.IsNullOrEmpty(value))
                return $"Changing {propertyName} aborted";

            try
            {
                update(value);
                return $"{propertyName} updated successfully";
            }
            catch (Exception e)
            {
                return e.Message;
            }
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

        private MenuItem[] GetDatabaseOptions()
        {
            var list = new List<MenuItem>
            {
                new MenuItem(
                    "Load from File",
                    submenu: new Menu(() => "Available backups", GetBackupMenuItems)
                ),
                new MenuItem(
                    "Save to File",
                    taskAction: () => DataHandler.WriteToDatabase(_facilities)
                ),
                new MenuItem("Back"),
            };
            return [.. list];
        }

        private MenuItem[] GetBackupMenuItems()
        {
            string[] availableFiles = DataHandler.GetAvailableBackups();
            var items = new List<MenuItem>();
            foreach (var item in availableFiles)
            {
                items.Add(
                    new MenuItem(
                        item,
                        taskAction: () => DataHandler.ReadFromDatabase(_facilities, item)
                    )
                );
            }
            items.Add(new MenuItem("Back"));
            return [.. items];
        }

        private Menu CreateFacilityMenu()
        {
            return new Menu(() => "Add Facility", GetFacilityMenuItems);
        }

        private MenuItem[] GetFacilityMenuItems()
        {
            return
            [
                new MenuItem("Add Garage", action: () => CreateFacility(GarageType.CarGarage)),
                new MenuItem("Add Hangar", action: () => CreateFacility(GarageType.Hangar)),
                new MenuItem("Add Marina", action: () => CreateFacility(GarageType.Marina)),
                new MenuItem("Add Mc garage", action: () => CreateFacility(GarageType.MCGarage)),
                new MenuItem("Back"),
            ];
        }

        private string? CreateFacility(GarageType garageType)
        {
            Console.Clear();

            string? name = GetFacilityName(garageType);

            if (name == null)
                return $"Adding {garageType} aborted";

            int? capacity = GetFacilityCapacity();

            if (capacity == null)
                return $"Adding {garageType} aborted";

            Garage facility = garageType switch
            {
                GarageType.CarGarage => new CarGarage(name, capacity.Value),
                GarageType.MCGarage => new MCGarage(name, capacity.Value),
                GarageType.Hangar => new Hangar(name, capacity.Value),
                GarageType.Marina => new Marina(name, capacity.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(garageType)),
            };

            _facilities.Add(facility);

            return $"Created new {garageType} successfully";
        }

        private static string? GetFacilityName(GarageType facilityType)
        {
            while (true)
            {
                Console.WriteLine(
                    $"Please enter a name for your {facilityType}. (Empty row to abort)"
                );
                string? input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }
                return null;
            }
        }

        private static int? GetFacilityCapacity()
        {
            while (true)
            {
                Console.WriteLine("How many spaces should the facility have? (Empty row to abort)");

                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    return null;

                if (int.TryParse(input, out int value) && value > 0)
                    return value;

                Console.WriteLine("Please enter a valid number greater than 0.");
            }
        }
    }
}
