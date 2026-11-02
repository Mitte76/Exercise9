using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exercise9.Menus;
using Exercise9.Services;
using Exercise9.Vehicles;

namespace Exercise9.UI
{
    internal static class GarageMenu
    {
        public static Task<string?> Show(int garageId, IGarageHandler garageHandler)
        {
            return new Menu<string>(
                () => $"Garage: {garageHandler.GetGarageSummary(garageId).Name}",
                () => GetMenuItems(garageId, garageHandler),
                extra: () =>
                {
                    GarageSummary summary = garageHandler.GetGarageSummary(garageId);
                    return $"Allowed vehicles: {string.Join(", ", summary.VehicleTypes)} | "
                        + $"Spaces: {summary.VehicleCount}/{summary.Capacity}";
                }
            ).ShowMenu();
        }

        private static MenuItem<string>[] GetMenuItems(int garageId, IGarageHandler garageHandler)
        {
            GarageSummary summary = garageHandler.GetGarageSummary(garageId);
            List<MenuItem<string>> items =
            [
                new MenuItem<string>(
                    "Manage parked vehicles",
                    taskAction: () => SelectParkedVehicle(garageId, garageHandler)
                ),
                new MenuItem<string>(
                    "Search vehicles",
                    taskAction: () => SearchVehicles(garageId, garageHandler)
                ),
            ];

            foreach (VehicleType vehicleType in summary.VehicleTypes)
            {
                items.Add(
                    new MenuItem<string>(
                        $"Park {vehicleType}",
                        action: () => AddVehicle(garageHandler, garageId, vehicleType)
                    )
                );
            }

            items.Add(
                new MenuItem<string>(
                    "Populate garage with random vehicles",
                    action: () => garageHandler.PopulateGarage(garageId)
                )
            );
            items.Add(new MenuItem<string>("Back"));
            return [.. items];
        }

        private static async Task<string?> SelectParkedVehicle(
            int garageId,
            IGarageHandler garageHandler
        )
        {
            IReadOnlyList<ParkedVehicleSummary> vehicles = garageHandler.GetParkedVehicles(
                garageId
            );
            if (vehicles.Count == 0)
                return "No vehicles are parked in this garage.";

            List<VehicleTypeSelection> typeSelections =
            [
                new VehicleTypeSelection(null, vehicles.Count),
                .. vehicles
                    .GroupBy(vehicle => vehicle.Type)
                    .Select(group => new VehicleTypeSelection(group.Key, group.Count())),
            ];
            VehicleTypeSelection? selectedType = await Menu<VehicleTypeSelection>
                .CreateSelectionMenu(
                    () => "Select Vehicle Type",
                    typeSelections,
                    selection => $"{selection.Type ?? "All vehicles"} ({selection.Count})"
                )
                .ShowMenu();

            if (selectedType == null)
                return null;

            const int pageSize = 20;
            int currentPage = 0;
            string vehicleTypeName = selectedType.Type ?? "All vehicles";
            string? actionMessage = null;

            while (true)
            {
                IReadOnlyList<ParkedVehicleSummary> filteredVehicles = GetParkedVehiclesForType(
                    garageId,
                    garageHandler,
                    selectedType.Type
                );
                if (filteredVehicles.Count == 0)
                    return actionMessage ?? "No vehicles remain in this list.";

                int totalPages = (filteredVehicles.Count + pageSize - 1) / pageSize;
                currentPage = Math.Clamp(currentPage, 0, totalPages - 1);
                ParkedVehicleSummary? selectedVehicle = await new Menu<ParkedVehicleSummary>(
                    () => $"Parked Vehicles - {vehicleTypeName}",
                    () =>
                    {
                        List<MenuItem<ParkedVehicleSummary>> items = [];
                        int startIndex = currentPage * pageSize;
                        int endIndex = Math.Min(startIndex + pageSize, filteredVehicles.Count);

                        for (int index = startIndex; index < endIndex; index++)
                        {
                            ParkedVehicleSummary vehicle = filteredVehicles[index];
                            items.Add(
                                new MenuItem<ParkedVehicleSummary>(
                                    $"#{vehicle.Number} {vehicle.Type} - {vehicle.Brand}",
                                    value: vehicle
                                )
                            );
                        }

                        items.Add(
                            new MenuItem<ParkedVehicleSummary>(
                                "Previous page",
                                action: () =>
                                {
                                    currentPage--;
                                    return null;
                                },
                                isEnabled: currentPage > 0
                            )
                        );

                        if (currentPage < totalPages - 1)
                            items.Add(
                                new MenuItem<ParkedVehicleSummary>(
                                    "Next page",
                                    action: () =>
                                    {
                                        currentPage++;
                                        return null;
                                    }
                                )
                            );

                        items.Add(new MenuItem<ParkedVehicleSummary>("Back"));
                        return [.. items];
                    },
                    extra: () =>
                        $"Page {currentPage + 1} of {totalPages}"
                        + (actionMessage == null ? "" : $" | {actionMessage}")
                ).ShowMenu();

                if (selectedVehicle == null)
                    return null;

                actionMessage = await ShowVehicleActions(garageId, garageHandler, selectedVehicle);
            }
        }

        private static IReadOnlyList<ParkedVehicleSummary> GetParkedVehiclesForType(
            int garageId,
            IGarageHandler garageHandler,
            string? type
        )
        {
            IReadOnlyList<ParkedVehicleSummary> vehicles = garageHandler.GetParkedVehicles(
                garageId
            );
            return type == null
                ? vehicles
                : vehicles.Where(vehicle => vehicle.Type == type).ToArray();
        }

        private static Task<string?> ShowVehicleActions(
            int garageId,
            IGarageHandler garageHandler,
            ParkedVehicleSummary selectedVehicle
        )
        {
            return new Menu<string>(
                () => $"{selectedVehicle.Type}: {selectedVehicle.Brand}",
                () =>
                    [
                        new MenuItem<string>(
                            "Edit vehicle",
                            valueFactory: () =>
                                EditVehicle(garageHandler, garageId, selectedVehicle.Number)
                        ),
                        new MenuItem<string>(
                            "Remove vehicle",
                            valueFactory: () =>
                                RemoveVehicle(garageHandler, garageId, selectedVehicle)
                        ),
                        new MenuItem<string>("Back"),
                    ],
                extra: () => $"{selectedVehicle.Color} | {selectedVehicle.Details}"
            ).ShowMenu();
        }

        private static async Task<string?> SearchVehicles(
            int garageId,
            IGarageHandler garageHandler
        )
        {
            string[] availableCriteria =
            [
                "Brand",
                "Color",
                "LicensePlateNumber",
                "VehicleType",
                "NumberOfWheels",
                "FuelType",
                "CylinderVolume",
                "Length",
                "NumberOfEngines",
                "NumberOfSeats",
            ];
            Dictionary<string, List<string>> criteria = new(StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                string? input = ReadSearchCriterion(availableCriteria, criteria);
                if (input == null)
                    return "Search cancelled.";
                if (string.IsNullOrWhiteSpace(input))
                    break;

                string[] parts = input.Split(':', 2);
                if (
                    parts.Length != 2
                    || string.IsNullOrWhiteSpace(parts[0])
                    || string.IsNullOrWhiteSpace(parts[1])
                )
                    return "Enter criteria in the format Criterion: Value.";

                string enteredCriterion = parts[0].Trim();
                string? matchingCriterion = availableCriteria.FirstOrDefault(criterion =>
                    string.Equals(criterion, enteredCriterion, StringComparison.OrdinalIgnoreCase)
                );
                if (matchingCriterion == null)
                    return $"Unknown criterion '{enteredCriterion}'.";
                if (!criteria.TryGetValue(matchingCriterion, out List<string>? values))
                {
                    values = [];
                    criteria.Add(matchingCriterion, values);
                }
                values.Add(parts[1].Trim());
            }

            IReadOnlyDictionary<string, IReadOnlyList<string>> searchCriteria =
                criteria.ToDictionary(
                    criterion => criterion.Key,
                    criterion => (IReadOnlyList<string>)criterion.Value,
                    StringComparer.OrdinalIgnoreCase
                );
            string selectedCriteria =
                criteria.Count == 0
                    ? "All vehicles"
                    : string.Join(
                        "; ",
                        criteria.Select(criterion =>
                            $"{criterion.Key}: {string.Join(", ", criterion.Value)}"
                        )
                    );
            string? actionMessage = null;

            while (true)
            {
                IReadOnlyList<ParkedVehicleSummary> matches = garageHandler.SearchVehicles(
                    garageId,
                    new VehicleSearchCriteria(searchCriteria)
                );
                if (matches.Count == 0)
                    return actionMessage ?? "No matching vehicles were found.";

                ParkedVehicleSummary? selectedVehicle = await Menu<ParkedVehicleSummary>
                    .CreateSelectionMenu(
                        () => "Search Results",
                        matches,
                        vehicle =>
                            $"#{vehicle.Number} {vehicle.Type} - {vehicle.Brand}, {vehicle.Color} ({vehicle.Details})",
                        extra: () =>
                            $"Criteria: {selectedCriteria} | Matches: {matches.Count}"
                            + (actionMessage == null ? "" : $" | {actionMessage}")
                    )
                    .ShowMenu();
                if (selectedVehicle == null)
                    return "Search results closed.";

                actionMessage = await ShowVehicleActions(garageId, garageHandler, selectedVehicle);
            }
        }

        private static string? ReadSearchCriterion(
            string[] availableCriteria,
            Dictionary<string, List<string>> selectedCriteria
        )
        {
            string input = "";
            string[] completionMatches = [];
            int completionIndex = -1;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Enter criteria as Criterion: Value.");
                Console.WriteLine("Leave an empty line to start the search.");
                Console.WriteLine(
                    "Press Tab to complete a criterion; press Tab again to cycle suggestions."
                );

                if (selectedCriteria.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nSelected criteria:");
                    foreach (KeyValuePair<string, List<string>> criterion in selectedCriteria)
                        Console.WriteLine($"{criterion.Key}: {string.Join(", ", criterion.Value)}");
                    Console.ResetColor();
                }

                string criterionPrefix = input.Split(':', 2)[0].Trim();
                string[] suggestions = availableCriteria
                    .Where(criterion =>
                        criterion.StartsWith(criterionPrefix, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray();

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
                Console.ResetColor();
                Console.WriteLine();
                Console.Write(input);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input;
                }

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return null;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                        input = input[..^1];
                    completionMatches = [];
                    completionIndex = -1;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    bool cyclingCompletion =
                        completionMatches.Length > 1
                        && completionIndex >= 0
                        && string.Equals(
                            input,
                            $"{completionMatches[completionIndex]}: ",
                            StringComparison.OrdinalIgnoreCase
                        );
                    if (cyclingCompletion)
                    {
                        completionIndex = (completionIndex + 1) % completionMatches.Length;
                    }
                    else
                    {
                        if (input.Contains(':') || suggestions.Length == 0)
                            continue;

                        completionMatches = suggestions;
                        completionIndex = 0;
                    }

                    input = $"{completionMatches[completionIndex]}: ";
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                    completionMatches = [];
                    completionIndex = -1;
                }
            }
        }

        private static string EditVehicle(
            IGarageHandler garageHandler,
            int garageId,
            int vehicleNumber
        )
        {
            try
            {
                Vehicle? currentVehicle = garageHandler.GetVehicle(garageId, vehicleNumber);
                if (currentVehicle == null)
                    return "The selected vehicle is no longer parked in this garage.";

                VehicleType vehicleType = currentVehicle switch
                {
                    Airplane => VehicleType.Airplane,
                    Boat => VehicleType.Boat,
                    Bus => VehicleType.Bus,
                    Car => VehicleType.Car,
                    Motorcycle => VehicleType.Motorcycle,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(currentVehicle),
                        $"Unsupported vehicle type: {currentVehicle.GetType().Name}"
                    ),
                };
                Vehicle replacement = VehicleForm.Create(
                    vehicleType,
                    currentVehicle,
                    registrationNumber => garageHandler.ValidateLicensePlateNumber(
                        registrationNumber,
                        currentVehicle
                    )
                );
                return garageHandler.EditVehicle(garageId, vehicleNumber, replacement);
            }
            catch (System.OperationCanceledException)
            {
                return "Vehicle edit cancelled.";
            }
            catch (System.ArgumentException exception)
            {
                return $"Could not edit vehicle: {exception.Message}";
            }
        }

        private static string RemoveVehicle(
            IGarageHandler garageHandler,
            int garageId,
            ParkedVehicleSummary vehicle
        )
        {
            Console.Write($"Remove {vehicle.Type} {vehicle.Brand}? (Y/N): ");
            if (Console.ReadKey(true).Key != ConsoleKey.Y)
                return "Vehicle removal cancelled.";

            return garageHandler.RemoveVehicle(garageId, vehicle.Number);
        }

        private static string AddVehicle(
            IGarageHandler garageHandler,
            int garageId,
            VehicleType vehicleType
        )
        {
            try
            {
                GarageSummary garage = garageHandler.GetGarageSummary(garageId);
                if (garage.VehicleCount >= garage.Capacity)
                    return "The garage is full.";

                Vehicle vehicle = VehicleForm.Create(vehicleType);
                return garageHandler.AddVehicle(garageId, vehicle);
            }
            catch (System.OperationCanceledException)
            {
                return "Vehicle creation cancelled.";
            }
            catch (System.ArgumentException exception)
            {
                return $"Could not create vehicle: {exception.Message}";
            }
        }

        private sealed record VehicleTypeSelection(string? Type, int Count);
    }
}
