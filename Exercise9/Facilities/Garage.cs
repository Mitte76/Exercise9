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
            return vehicleTypes.Length == 0 ? "Empty" : string.Join(", ", VehicleTypes);
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
