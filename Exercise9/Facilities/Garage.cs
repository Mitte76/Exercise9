using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    internal class Garage<T> : IEnumerable<T>
        where T : Vehicle
    {
        public const int MaximumCapacity = 100_000;

        private readonly Vehicle?[] vehicles;

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
            vehicles = new Vehicle?[capacity];
        }

        public string Name { get; }
        public int Capacity { get; }
        public IReadOnlyList<VehicleType> VehicleTypes { get; }

        public bool TryParkVehicle(Vehicle vehicle)
        {
            if (!Accepts(vehicle) || HasDuplicateRegistrationNumber(vehicle))
                return false;

            int availableSlot = Array.FindIndex(vehicles, parkedVehicle => parkedVehicle is null);
            if (availableSlot < 0)
                return false;

            vehicles[availableSlot] = vehicle;
            return true;
        }

        internal Vehicle?[] GetVehicleSlots() => [.. vehicles];

        internal bool TryParkVehicleAt(int position, Vehicle vehicle)
        {
            int index = position - 1;
            if (
                index < 0
                || index >= vehicles.Length
                || vehicles[index] is not null
                || !Accepts(vehicle)
                || HasDuplicateRegistrationNumber(vehicle)
            )
                return false;

            vehicles[index] = vehicle;
            return true;
        }

        private bool Accepts(Vehicle vehicle)
        {
            if (vehicle is not T)
                return false;

            VehicleType? vehicleType = vehicle switch
            {
                Airplane => VehicleType.Airplane,
                Boat => VehicleType.Boat,
                Bus => VehicleType.Bus,
                Car => VehicleType.Car,
                Motorcycle => VehicleType.Motorcycle,
                _ => null,
            };

            return vehicleType != null && VehicleTypes.Contains(vehicleType.Value);
        }

        private bool HasDuplicateRegistrationNumber(Vehicle vehicle, int ignoredPosition = -1)
        {
            for (int index = 0; index < vehicles.Length; index++)
            {
                Vehicle? parkedVehicle = vehicles[index];
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

        public bool TryReplaceVehicle(int position, Vehicle vehicle)
        {
            int index = position - 1;
            if (
                index < 0
                || index >= vehicles.Length
                || vehicles[index] is null
                || !Accepts(vehicle)
                || HasDuplicateRegistrationNumber(vehicle, index)
            )
                return false;

            vehicles[index] = vehicle;
            return true;
        }

        public bool TryRemoveVehicle(int position)
        {
            int index = position - 1;
            if (index < 0 || index >= vehicles.Length || vehicles[index] is null)
                return false;

            vehicles[index] = null;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (Vehicle? vehicle in vehicles)
            {
                if (vehicle is T typedVehicle)
                    yield return typedVehicle;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
