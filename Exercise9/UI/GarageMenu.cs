using Exercise9.Menus;
using Exercise9.Services;
using Exercise9.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                )
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
            IReadOnlyList<ParkedVehicleSummary> vehicles = garageHandler.GetParkedVehicles(garageId);
            if (vehicles.Count == 0)
                return "No vehicles are parked in this garage.";


            List<VehicleTypeSelection> typeSelections =
            [
                new VehicleTypeSelection(null, vehicles.Count),
                .. vehicles.GroupBy(vehicle => vehicle.Type)
                    .Select(group => new VehicleTypeSelection(group.Key, group.Count())),
            ];
            VehicleTypeSelection? selectedType = await Menu<VehicleTypeSelection>.CreateSelectionMenu(
                () => "Select Vehicle Type",
                typeSelections,
                selection => $"{selection.Type ?? "All vehicles"} ({selection.Count})"
            ).ShowMenu();

            if (selectedType == null)
                return null;

            IReadOnlyList<ParkedVehicleSummary> filteredVehicles = selectedType.Type is string type
                ? vehicles.Where(vehicle => vehicle.Type == type).ToArray()
                : vehicles;
            const int pageSize = 20;
            int currentPage = 0;
            int totalPages = (filteredVehicles.Count + pageSize - 1) / pageSize;
            string vehicleTypeName = selectedType.Type?.ToString() ?? "All vehicles";

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

                    if (currentPage > 0)
                        items.Add(
                            new MenuItem<ParkedVehicleSummary>(
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
                extra: () => $"Page {currentPage + 1} of {totalPages}"
            ).ShowMenu();

            if (selectedVehicle == null)
                return null;

            return await new Menu<string>(
                () => $"{selectedVehicle.Type}: {selectedVehicle.Brand}",
                () =>
                [
                    new MenuItem<string>(
                        "Edit vehicle",
                        valueFactory: () => EditVehicle(garageHandler, garageId, selectedVehicle.Number)
                    ),
                    new MenuItem<string>(
                        "Remove vehicle",
                        valueFactory: () => RemoveVehicle(garageHandler, garageId, selectedVehicle)
                    ),
                    new MenuItem<string>("Back")
                ],
                extra: () => $"{selectedVehicle.Color} | {selectedVehicle.Details}"
            ).ShowMenu();
        }

        private static string EditVehicle(
            IGarageHandler garageHandler,
            int garageId,
            int vehicleNumber
        )
        {
            try
            {
                return garageHandler.EditVehicle(garageId, vehicleNumber);
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
                return garageHandler.AddVehicle(garageId, vehicleType);
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
