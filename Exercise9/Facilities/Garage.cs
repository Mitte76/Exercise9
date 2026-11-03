using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    public class Garage<T> : IEnumerable<T>
        where T : Vehicle
    {
        public const int MaximumCapacity = 10000;

        private readonly T?[] vehicles;
        private readonly ParkingSpot[] parkingSpots;

        public Garage(string name, int capacity, VehicleType[] vehicleTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(vehicleTypes);
            if (capacity <= 0 || capacity > MaximumCapacity)
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    $"Capacity must be between 1 and {MaximumCapacity}."
                );

            Name = name;
            Capacity = capacity;
            VehicleTypes = Array.AsReadOnly(vehicleTypes.ToArray());
            vehicles = new T?[capacity * ParkingSpot.UnitsPerSpace];
            parkingSpots = Enumerable
                .Range(1, capacity)
                .Select(position => new ParkingSpot(position))
                .ToArray();
        }

        public string Name { get; }
        public int Capacity { get; }
        public IReadOnlyList<VehicleType> VehicleTypes { get; }
        public int AvailableParkingSpaceUnits => parkingSpots.Sum(spot => spot.AvailableUnits);
        public int OccupiedParkingSpaceUnits =>
            Capacity * ParkingSpot.UnitsPerSpace - AvailableParkingSpaceUnits;

        public bool HasSpaceFor(VehicleType vehicleType)
        {
            return VehicleTypes.Contains(vehicleType)
                && GetParkingSpaceUnits(vehicleType) <= AvailableParkingSpaceUnits;
        }

        public bool HasSpaceFor(T vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            return Accepts(vehicle)
                && GetRequiredParkingSpaceUnits(vehicle) <= AvailableParkingSpaceUnits;
        }

        public bool TryParkVehicle(T vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            if (!Accepts(vehicle) || HasDuplicateRegistrationNumber(vehicle))
                return false;

            int vehicleSlot = Array.FindIndex(vehicles, parkedVehicle => parkedVehicle is null);
            int requiredUnits = GetRequiredParkingSpaceUnits(vehicle);
            if (vehicleSlot < 0 || requiredUnits > AvailableParkingSpaceUnits)
                return false;

            vehicles[vehicleSlot] = vehicle;
            AllocateParkingSpace(vehicle, requiredUnits);
            return true;
        }

        internal T?[] GetVehicleSlots()
        {
            int lastOccupiedSlot = Array.FindLastIndex(vehicles, vehicle => vehicle is not null);
            return lastOccupiedSlot < 0 ? [] : vehicles[..(lastOccupiedSlot + 1)];
        }

        internal bool TryParkVehicleAt(int position, T vehicle)
        {
            int index = position - 1;
            if (
                index < 0
                || index >= vehicles.Length
                || vehicles[index] is not null
                || !Accepts(vehicle)
                || HasDuplicateRegistrationNumber(vehicle)
                || GetRequiredParkingSpaceUnits(vehicle) > AvailableParkingSpaceUnits
            )
                return false;

            vehicles[index] = vehicle;
            AllocateParkingSpace(vehicle, GetRequiredParkingSpaceUnits(vehicle));
            return true;
        }

        private bool Accepts(T vehicle)
        {
            VehicleType? vehicleType = vehicle switch
            {
                Airplane => VehicleType.Airplane,
                Boat => VehicleType.Boat,
                Bus => VehicleType.Bus,
                Car => VehicleType.Car,
                Motorcycle => VehicleType.Motorcycle,
                _ => null,
            };

            return vehicleType.HasValue && VehicleTypes.Contains(vehicleType.Value);
        }

        private static int GetParkingSpaceUnits(VehicleType vehicleType)
        {
            return ConvertParkingSpaceRequirementToUnits(vehicleType.GetParkingSpaceRequirement());
        }

        private static int GetRequiredParkingSpaceUnits(T vehicle)
        {
            return ConvertParkingSpaceRequirementToUnits(vehicle.ParkingSpaceRequirement);
        }

        private static int ConvertParkingSpaceRequirementToUnits(decimal requirement)
        {
            decimal units = requirement * ParkingSpot.UnitsPerSpace;
            decimal roundedUnits = decimal.Round(units, MidpointRounding.AwayFromZero);
            if (requirement <= 0 || Math.Abs(units - roundedUnits) > 0.000000001m)
                throw new InvalidOperationException(
                    "Vehicle parking-space requirements must be positive multiples of one third of a space."
                );

            return decimal.ToInt32(roundedUnits);
        }

        private void AllocateParkingSpace(T vehicle, int requiredUnits)
        {
            int remainingUnits = requiredUnits;
            bool requiresWholeSpaces = requiredUnits % ParkingSpot.UnitsPerSpace == 0;
            foreach (ParkingSpot spot in parkingSpots)
            {
                if (requiresWholeSpaces && spot.AvailableUnits != ParkingSpot.UnitsPerSpace)
                    continue;

                int allocation = Math.Min(remainingUnits, spot.AvailableUnits);
                if (allocation == 0)
                    continue;

                if (!spot.TryAllocate(vehicle, allocation))
                    throw new InvalidOperationException("Could not allocate parking space.");

                remainingUnits -= allocation;
                if (remainingUnits == 0)
                    return;
            }

            throw new InvalidOperationException("Not enough parking space is available.");
        }

        private void ReleaseParkingSpace(T vehicle)
        {
            foreach (ParkingSpot spot in parkingSpots)
                spot.Release(vehicle);
        }

        private bool HasDuplicateRegistrationNumber(T vehicle, int ignoredPosition = -1)
        {
            for (int index = 0; index < vehicles.Length; index++)
            {
                T? parkedVehicle = vehicles[index];
                if (
                    index != ignoredPosition
                    && parkedVehicle != null
                    && string.Equals(
                        parkedVehicle.LicensePlateNumber,
                        vehicle.LicensePlateNumber,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    return true;
            }

            return false;
        }

        public IReadOnlyList<int> GetParkingSpotNumbers(T vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            return parkingSpots
                .Where(spot => spot.ContainsVehicle(vehicle))
                .Select(spot => spot.Number)
                .ToArray();
        }

        public IEnumerable<(int Position, T Vehicle)> GetVehiclesWithPositions()
        {
            for (int index = 0; index < vehicles.Length; index++)
            {
                if (vehicles[index] is T vehicle)
                    yield return (index + 1, vehicle);
            }
        }

        public bool TryGetVehicle(int position, out T? vehicle)
        {
            int index = position - 1;
            if (index < 0 || index >= vehicles.Length || vehicles[index] is not T parkedVehicle)
            {
                vehicle = null;
                return false;
            }

            vehicle = parkedVehicle;
            return true;
        }

        public bool TryReplaceVehicle(int position, T vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            int index = position - 1;
            if (
                index < 0
                || index >= vehicles.Length
                || vehicles[index] is not T currentVehicle
                || !Accepts(vehicle)
                || HasDuplicateRegistrationNumber(vehicle, index)
                || GetRequiredParkingSpaceUnits(vehicle)
                    > AvailableParkingSpaceUnits + GetRequiredParkingSpaceUnits(currentVehicle)
            )
                return false;

            ReleaseParkingSpace(currentVehicle);
            vehicles[index] = vehicle;
            AllocateParkingSpace(vehicle, GetRequiredParkingSpaceUnits(vehicle));
            return true;
        }

        public bool TryRemoveVehicle(int position)
        {
            int index = position - 1;
            if (index < 0 || index >= vehicles.Length || vehicles[index] is null)
                return false;

            T vehicle = vehicles[index]!;
            ReleaseParkingSpace(vehicle);
            vehicles[index] = null;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T? vehicle in vehicles)
            {
                if (vehicle is not null)
                    yield return vehicle;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
