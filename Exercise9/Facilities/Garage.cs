using Exercise6.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Exercise6.Facilities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(CarGarage), "CarGarage")]
    [JsonDerivedType(typeof(Hangar), "Hangar")]
    [JsonDerivedType(typeof(Marina), "Marina")]
    [JsonDerivedType(typeof(MCGarage), "MCGarage")]
    internal abstract class Garage(string name, int capacity)
    {
        public abstract VehicleType[] VehicleTypes { get; }

        public string Name { get; private set; } = name;
        public int Capacity { get; private set; }

        public int OccupiedSlots => parkingSpots.Count(s => s.IsOccupied);

        public int AvailableSlots => parkingSpots.Count(s => !s.IsOccupied);

        private ParkingSpot[] parkingSpots = [.. Enumerable
            .Range(0, capacity)
            .Select(id => new ParkingSpot(id))];

        [JsonInclude]
        public ParkingSpot[] ParkingSpots
        {
            get => parkingSpots;
            private set => parkingSpots = value;
        }

        public Dictionary<VehicleType, int> GetVehicleTypeCounts()
        {
            var counts = VehicleTypes.ToDictionary(type => type, _ => 0);

            foreach (var group in ParkingSpots
                .Where(spot => spot.Vehicle is not null)
                .GroupBy(spot => spot.Vehicle switch
                {
                    Car => VehicleType.Car,
                    Bus => VehicleType.Bus,
                    Motorcycle => VehicleType.Motorcycle,
                    Boat => VehicleType.Boat,
                    Airplane => VehicleType.Airplane,
                    _ => throw new ArgumentOutOfRangeException(),
                }))
            {
                counts[group.Key] = group.Count();
            }

            return counts;
        }

        public ParkingSpot? GetFirstAvailableParkignSpot()
        {
            return parkingSpots.FirstOrDefault(s=> !s.IsOccupied);
        }

        public List<ParkingSpot> GetAllOccupiesSpots()
        {
            return [.. parkingSpots.Where(s => s.IsOccupied)];
        }

        public string ParkCar(Vehicle vehicle)
        {
            ParkingSpot? spot = GetFirstAvailableParkignSpot();
            if (spot == null)
                return "No available parking spot found";
            spot.AddVehicle(vehicle);
            return $"Car parked successfully {AvailableSlots} slots available. {OccupiedSlots} Vehicles currenty parked";
        }
    }
}
