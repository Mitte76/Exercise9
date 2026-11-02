using System.Text.Json.Serialization;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    public class ParkingSpot(int id)
    {
        public int Id { get; } = id;

        public bool IsOccupied => Vehicle != null;

        [JsonInclude]
        public Vehicle? Vehicle { get; private set; }

        public void AddVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public void RemoveVehicle()
        {
            Vehicle = null;
        }
    }
}
