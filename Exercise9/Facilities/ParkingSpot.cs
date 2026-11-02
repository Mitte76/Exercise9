using System;
using System.Collections.Generic;
using System.Linq;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    public sealed class ParkingSpot
    {
        public const int UnitsPerSpace = 3;

        private readonly List<(Vehicle Vehicle, int Units)> allocations = [];

        internal ParkingSpot(int number)
        {
            Number = number;
        }

        public int Number { get; }
        public int CapacityUnits => UnitsPerSpace;
        public int UsedUnits => allocations.Sum(allocation => allocation.Units);
        public int AvailableUnits => CapacityUnits - UsedUnits;
        public IReadOnlyList<Vehicle> Vehicles => allocations.Select(allocation => allocation.Vehicle).ToArray();

        internal bool ContainsVehicle(Vehicle vehicle)
        {
            return allocations.Any(allocation => ReferenceEquals(allocation.Vehicle, vehicle));
        }

        internal bool TryAllocate(Vehicle vehicle, int units)
        {
            if (units <= 0 || units > AvailableUnits)
                return false;

            allocations.Add((vehicle, units));
            return true;
        }

        internal void Release(Vehicle vehicle)
        {
            allocations.RemoveAll(allocation => ReferenceEquals(allocation.Vehicle, vehicle));
        }
    }
}
