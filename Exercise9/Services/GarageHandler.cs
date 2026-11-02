using System;
using System.Collections.Generic;
using System.IO;
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
        int OccupiedParkingSpaceUnits,
        IReadOnlyList<VehicleType> VehicleTypes
    );

    internal sealed record ParkedVehicleSummary(
        int Number,
        IReadOnlyList<int> ParkingSpotNumbers,
        string Type,
        string Brand,
        string Color,
        string Details
    );

    internal sealed record VehicleSearchCriteria(
        IReadOnlyDictionary<string, IReadOnlyList<string>> Criteria
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

        public IReadOnlyList<GarageSnapshot> CreateBackupSnapshot()
        {
            return _garages
                .Values.Select(garage => new GarageSnapshot(
                    garage.Name,
                    garage.Capacity,
                    garage.VehicleTypes.ToArray(),
                    garage.GetVehicleSlots()
                ))
                .ToArray();
        }

        public void RestoreBackupSnapshot(IReadOnlyList<GarageSnapshot> snapshots)
        {
            ArgumentNullException.ThrowIfNull(snapshots);
            Dictionary<int, Garage<Vehicle>> restoredGarages = [];
            int garageId = 1;

            foreach (GarageSnapshot snapshot in snapshots)
            {
                if (
                    snapshot is null
                    || string.IsNullOrWhiteSpace(snapshot.Name)
                    || snapshot.Capacity <= 0
                    || snapshot.Capacity > Garage<Vehicle>.MaximumCapacity
                    || snapshot.VehicleTypes is null
                    || snapshot.VehicleTypes.Length == 0
                    || snapshot.Vehicles is null
                    || snapshot.Vehicles.Length > snapshot.Capacity * ParkingSpot.UnitsPerSpace
                    || snapshot.VehicleTypes.Any(type => !Enum.IsDefined(type))
                )
                    throw new InvalidDataException("The backup contains an invalid garage.");

                Garage<Vehicle> garage = new(
                    snapshot.Name,
                    snapshot.Capacity,
                    snapshot.VehicleTypes
                );
                for (int position = 0; position < snapshot.Vehicles.Length; position++)
                {
                    Vehicle? vehicle = snapshot.Vehicles[position];
                    if (vehicle is null)
                        continue;

                    if (!garage.TryParkVehicleAt(position + 1, vehicle))
                        throw new InvalidDataException(
                            "The backup contains an invalid parked vehicle."
                        );

                    Vehicle.ValidateLicensePlateNumber(
                        vehicle.LicensePlateNumber,
                        [.. restoredGarages.Values, garage],
                        vehicle
                    );
                }

                restoredGarages.Add(garageId++, garage);
            }

            _garages.Clear();
            foreach (KeyValuePair<int, Garage<Vehicle>> entry in restoredGarages)
                _garages.Add(entry.Key, entry.Value);
            _nextGarageId = garageId;
        }

        public GarageSummary GetGarageSummary(int garageId)
        {
            return CreateSummary(garageId, FindGarage(garageId));
        }

        public IReadOnlyList<ParkedVehicleSummary> GetParkedVehicles(int garageId)
        {
            Garage<Vehicle> garage = FindGarage(garageId);
            return garage
                .GetVehiclesWithPositions()
                .Select(vehicle =>
                    CreateVehicleSummary(
                        vehicle.Position,
                        garage.GetParkingSpotNumbers(vehicle.Vehicle),
                        vehicle.Vehicle
                    )
                )
                .ToArray();
        }

        public IReadOnlyList<VehicleType> GetParkableVehicleTypes(int garageId)
        {
            Garage<Vehicle> garage = FindGarage(garageId);
            return garage.VehicleTypes.Where(garage.HasSpaceFor).ToArray();
        }

        public IReadOnlyList<ParkedVehicleSummary> SearchVehicles(
            int garageId,
            VehicleSearchCriteria criteria
        )
        {
            ArgumentNullException.ThrowIfNull(criteria);
            ArgumentNullException.ThrowIfNull(criteria.Criteria);
            Garage<Vehicle> garage = FindGarage(garageId);
            return garage
                .GetVehiclesWithPositions()
                .Where(entry => MatchesSearchCriteria(entry.Vehicle, criteria.Criteria))
                .Select(entry =>
                    CreateVehicleSummary(
                        entry.Position,
                        garage.GetParkingSpotNumbers(entry.Vehicle),
                        entry.Vehicle
                    )
                )
                .ToArray();
        }

        public GarageSummary AddGarage(string name, int capacity, VehicleType[] vehicleTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(vehicleTypes);
            if (capacity <= 0 || capacity > Garage<Vehicle>.MaximumCapacity)
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    $"Capacity must be between 1 and {Garage<Vehicle>.MaximumCapacity}."
                );
            if (
                vehicleTypes.Length == 0
                || vehicleTypes.Any(type => !Enum.IsDefined(type))
                || vehicleTypes.Distinct().Count() != vehicleTypes.Length
            )
                throw new ArgumentException(
                    "Select one or more valid, unique vehicle types.",
                    nameof(vehicleTypes)
                );

            int garageId = _nextGarageId++;
            Garage<Vehicle> garage = new(name, capacity, vehicleTypes);
            _garages.Add(garageId, garage);
            return CreateSummary(garageId, garage);
        }

        public Vehicle? GetVehicle(int garageId, int vehicleNumber)
        {
            return FindGarage(garageId).TryGetVehicle(vehicleNumber, out Vehicle? vehicle)
                ? vehicle
                : null;
        }

        public string AddVehicle(int garageId, Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            Garage<Vehicle> garage = FindGarage(garageId);
            VehicleType? vehicleType = GetVehicleType(vehicle);
            if (vehicleType == null || !garage.VehicleTypes.Contains(vehicleType.Value))
                return $"This garage does not accept {vehicle.GetType().Name}.";

            if (!garage.HasSpaceFor(vehicle))
                return $"There is not enough parking space for this {vehicleType}.";

            Vehicle.ValidateLicensePlateNumber(vehicle.LicensePlateNumber, [.. _garages.Values]);
            if (!garage.TryParkVehicle(vehicle))
                return $"Could not park the {vehicleType}.";

            string parkingSpots = string.Join(", ", garage.GetParkingSpotNumbers(vehicle));
            return $"{vehicleType} parked in {garage.Name} at parking spot(s) {parkingSpots}.";
        }

        public string PopulateGarage(int garageId)
        {
            return GaragePopulationService.PopulateGarage(
                FindGarage(garageId),
                [.. _garages.Values]
            );
        }

        public string ValidateLicensePlateNumber(
            string registrationNumber,
            Vehicle? vehicleBeingUpdated = null
        )
        {
            return Vehicle.ValidateLicensePlateNumber(
                registrationNumber,
                [.. _garages.Values],
                vehicleBeingUpdated
            );
        }

        public string EditVehicle(int garageId, int vehicleNumber, Vehicle replacement)
        {
            ArgumentNullException.ThrowIfNull(replacement);
            Garage<Vehicle> garage = FindGarage(garageId);
            if (
                !garage.TryGetVehicle(vehicleNumber, out Vehicle? currentVehicle)
                || currentVehicle == null
            )
                return "The selected vehicle is no longer parked in this garage.";

            if (GetVehicleType(currentVehicle) != GetVehicleType(replacement))
                return "The replacement must be the same vehicle type.";

            Vehicle.ValidateLicensePlateNumber(
                replacement.LicensePlateNumber,
                [.. _garages.Values],
                currentVehicle
            );
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
                garage.OccupiedParkingSpaceUnits,
                Array.AsReadOnly(garage.VehicleTypes.ToArray())
            );
        }

        private static ParkedVehicleSummary CreateVehicleSummary(
            int number,
            IReadOnlyList<int> parkingSpotNumbers,
            Vehicle vehicle
        )
        {
            string typeDetails = vehicle switch
            {
                Airplane airplane => $"Engines: {airplane.NumberOfEngines}",
                Boat boat => $"Length: {boat.Length}",
                Bus bus => $"Seats: {bus.NumberOfSeats}",
                Car car => $"Fuel: {car.FuelType}",
                Motorcycle motorcycle => $"Engine: {motorcycle.CylinderVolume} cc",
                _ => string.Empty,
            };
            string details =
                $"Registration: {vehicle.LicensePlateNumber}; Wheels: {vehicle.NumberOfWheels}; "
                + $"Parking spaces: {vehicle.ParkingSpaceRequirement:0.##}; {typeDetails}";

            return new ParkedVehicleSummary(
                number,
                parkingSpotNumbers,
                vehicle.GetType().Name,
                vehicle.Brand,
                vehicle.Color,
                details
            );
        }

        private static bool MatchesSearchCriteria(
            Vehicle vehicle,
            IReadOnlyDictionary<string, IReadOnlyList<string>> criteria
        )
        {
            foreach (KeyValuePair<string, IReadOnlyList<string>> criterion in criteria)
            {
                string? value = GetSearchableValue(vehicle, criterion.Key);
                if (
                    criterion.Value.Count == 0
                    || value == null
                    || !criterion.Value.Any(searchValue =>
                        value.Contains(
                            criterion.Key == "LicensePlateNumber"
                                ? searchValue.Trim().Replace(" ", "")
                                : searchValue,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                    return false;
            }

            return true;
        }

        private static string? GetSearchableValue(Vehicle vehicle, string criterion)
        {
            return criterion switch
            {
                "Brand" => vehicle.Brand,
                "Color" => vehicle.Color,
                "LicensePlateNumber" => vehicle.LicensePlateNumber,
                "VehicleType" => GetVehicleType(vehicle)?.ToString(),
                "NumberOfWheels" => vehicle.NumberOfWheels.ToString(),
                "FuelType" => vehicle is Car car ? car.FuelType.ToString() : null,
                "CylinderVolume" => vehicle is Motorcycle motorcycle
                    ? motorcycle.CylinderVolume.ToString()
                    : null,
                "Length" => vehicle is Boat boat ? boat.Length.ToString() : null,
                "NumberOfEngines" => vehicle is Airplane airplane
                    ? airplane.NumberOfEngines.ToString()
                    : null,
                "NumberOfSeats" => vehicle is Bus bus ? bus.NumberOfSeats.ToString() : null,
                _ => throw new ArgumentException($"Unknown vehicle search criterion: {criterion}"),
            };
        }

        private static VehicleType? GetVehicleType(Vehicle vehicle)
        {
            return vehicle switch
            {
                Airplane => VehicleType.Airplane,
                Boat => VehicleType.Boat,
                Bus => VehicleType.Bus,
                Car => VehicleType.Car,
                Motorcycle => VehicleType.Motorcycle,
                _ => null,
            };
        }
    }
}
