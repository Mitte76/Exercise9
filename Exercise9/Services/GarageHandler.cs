using Exercise9.Facilities;
using Exercise9.Vehicles;
using System.Collections.Generic;
using System.Linq;

namespace Exercise9.Services
{
    internal class GarageHandler()
    {
        private readonly List<Garage<Vehicle>> _garages = [];

        public List<string> GetGarageSummaries()
        {
            return _garages.Select(garage => $"{garage.Name} – {garage.Capacity} platser").ToList();
        }

        public int GetGarageCount() { return _garages.Count; }

        public List<Garage<Vehicle>> GetGarages() { return _garages; }

        public static void GetVehicleCount(Garage<Vehicle> garage)
        {
            int test = garage.Count();
        }

        public static string GetCurrentVehicleContent(Garage<Vehicle> garage)
        {
            return garage.GetAllowedVehicles();
        }

        public void AddGarage()
        {
            _garages.Add(new Garage<Vehicle>("test", 50, [VehicleType.Car]));
        }

        public Garage<Vehicle> AddGarage(string name, int capacity, VehicleType[] vehicleTypes)
        {
            Garage<Vehicle> garage = new(name, capacity, vehicleTypes);
            _garages.Add(garage);
            return garage;
        }

        public int GarageCount()
        {
            return _garages.Count;
        }

    }
}
