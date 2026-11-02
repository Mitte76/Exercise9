using System;
using System.Collections.Generic;
using System.Linq;
using Exercise9.Facilities;
using Exercise9.Vehicles;

namespace Exercise9.Services
{
    internal sealed record GarageSummary(
        int Id,
        string Name,
        int Capacity,
        int VehicleCount,
        IReadOnlyList<VehicleType> VehicleTypes
    );

    internal sealed record ParkedVehicleSummary(
        int Number,
        string Type,
        string Brand,
        string Color,
        string Details
    );

    internal class GarageHandler() : IGarageHandler
    {
        private readonly Dictionary<int, Garage<Vehicle>> _garages = [];
        private int _nextGarageId = 1;

        public int GarageCount => _garages.Count;

        public IReadOnlyList<GarageSummary> GetGarages()
        {
            return _garages.Select(entry => CreateSummary(entry.Key, entry.Value)).ToArray();
        }

        public GarageSummary GetGarageSummary(int garageId)
        {
            return CreateSummary(garageId, FindGarage(garageId));
        }

        public IReadOnlyList<ParkedVehicleSummary> GetParkedVehicles(int garageId)
        {
            return FindGarage(garageId)
                .GetVehiclesWithPositions()
                .Select(vehicle => CreateVehicleSummary(vehicle.Position, vehicle.Vehicle))
                .ToArray();
        }

        public GarageSummary AddGarage(string name, int capacity, VehicleType[] vehicleTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(vehicleTypes);
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            if (vehicleTypes.Length == 0)
                throw new ArgumentException("At least one vehicle type must be allowed.", nameof(vehicleTypes));

            int garageId = _nextGarageId++;
            Garage<Vehicle> garage = new(name, capacity, vehicleTypes);
            _garages.Add(garageId, garage);
            return CreateSummary(garageId, garage);
        }

        public string AddVehicle(int garageId, VehicleType vehicleType)
        {
            Garage<Vehicle> garage = FindGarage(garageId);
            if (garage.Count() >= garage.Capacity)
                return "The garage is full.";
            if (!garage.VehicleTypes.Contains(vehicleType))
                return $"This garage does not accept {vehicleType}.";

            Vehicle vehicle = CreateVehicle(vehicleType, [.. _garages.Values]);

            return garage.TryParkVehicle(vehicle)
                ? $"{vehicleType} parked in {garage.Name}."
                : $"Could not park the {vehicleType}.";
        }
        public string PopulateGarage(int garageId)
        {
            return GaragePopulationService.PopulateGarage(
                FindGarage(garageId),
                [.. _garages.Values]
            );
        }

        public string EditVehicle(int garageId, int vehicleNumber)
        {
            Garage<Vehicle> garage = FindGarage(garageId);
            if (!garage.TryGetVehicle(vehicleNumber, out Vehicle? currentVehicle) || currentVehicle == null)
                return "The selected vehicle is no longer parked in this garage.";

            Vehicle replacement = CreateVehicle(currentVehicle, [.. _garages.Values]);
            return garage.TryReplaceVehicle(vehicleNumber, replacement)
                ? $"{replacement.GetType().Name} updated in {garage.Name}."
                : "Could not update the selected vehicle.";
        }

        public string RemoveVehicle(int garageId, int vehicleNumber)
        {
            Garage<Vehicle> garage = FindGarage(garageId);
            return garage.TryRemoveVehicle(vehicleNumber)
                ? "Vehicle removed from the garage."
                : "The selected vehicle is no longer parked in this garage.";
        }

        private static Vehicle CreateVehicle(VehicleType vehicleType, List<Garage<Vehicle>> garages)
        {
            return vehicleType switch
            {
                VehicleType.Airplane => Airplane.Create(),
                VehicleType.Boat => Boat.Create(),
                VehicleType.Bus => Bus.Create(garages),
                VehicleType.Car => Car.Create(garages),
                VehicleType.Motorcycle => Motorcycle.Create(garages),
                _ => throw new ArgumentOutOfRangeException(nameof(vehicleType)),
            };
        }

        private static Vehicle CreateVehicle(Vehicle currentVehicle, List<Garage<Vehicle>> garages)
        {
            return currentVehicle switch
            {
                Airplane => Airplane.Create(),
                Boat => Boat.Create(),
                Bus => Bus.Create(garages),
                Car => Car.Create(garages),
                Motorcycle => Motorcycle.Create(garages),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(currentVehicle),
                    $"Unsupported vehicle type: {currentVehicle.GetType().Name}"
                ),
            };
        }

        private Garage<Vehicle> FindGarage(int garageId)
        {
            if (!_garages.TryGetValue(garageId, out Garage<Vehicle>? garage))
                throw new KeyNotFoundException($"Garage with ID {garageId} was not found.");

            return garage;
        }

        private static GarageSummary CreateSummary(int garageId, Garage<Vehicle> garage)
        {
            return new GarageSummary(
                garageId,
                garage.Name,
                garage.Capacity,
                garage.Count(),
                Array.AsReadOnly(garage.VehicleTypes.ToArray())
            );
        }

        private static ParkedVehicleSummary CreateVehicleSummary(int number, Vehicle vehicle)
        {
            string details = vehicle switch
            {
                Airplane airplane => $"Engines: {airplane.NumberOfEngines}",
                Boat boat => $"Length: {boat.Length}",
                Bus bus => $"Plate: {bus.LicensePlateNumber}; Seats: {bus.NumberOfSeats}",
                Car car => $"Plate: {car.LicensePlateNumber}; Fuel: {car.FuelType}",
                Motorcycle motorcycle =>
                    $"Plate: {motorcycle.LicensePlateNumber}; Engine: {motorcycle.CylinderVolume} cc",
                _ => string.Empty,
            };

            return new ParkedVehicleSummary(
                number,
                vehicle.GetType().Name,
                vehicle.Brand,
                vehicle.Color,
                details
            );
        }
    }
}
