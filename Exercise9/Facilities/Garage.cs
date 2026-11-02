using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    internal class Garage<T>(string name, int capacity, VehicleType[] vehicleTypes) : IEnumerable<T>
        where T : Vehicle
    {

        private readonly Vehicle[] vehicles = new Vehicle[capacity];

        public string Name { get; private set; } = name;
        public int Capacity { get; private set; } = capacity;
        public IReadOnlyList<VehicleType> VehicleTypes { get; } = Array.AsReadOnly(vehicleTypes);

        public string GetAllowedVehicles()
        {
            return VehicleTypes.Count == 0 ? "Empty" : string.Join(", ", VehicleTypes);
        }

        public bool TryParkVehicle(Vehicle vehicle)
        {
            if (!Accepts(vehicle))
                return false;

            int availableSlot = Array.FindIndex(vehicles, parkedVehicle => parkedVehicle is null);
            if (availableSlot < 0)
                return false;

            vehicles[availableSlot] = vehicle;
            return true;
        }

        private bool Accepts(Vehicle vehicle)
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

            return vehicleType != null && VehicleTypes.Contains(vehicleType.Value);
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

            vehicles[index] = null!;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T vehicle in vehicles)
            {
                if (vehicle is not null)
                {
                    yield return vehicle;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
