using System.Collections.Generic;
using Exercise9.Vehicles;

namespace Exercise9.Services
{
    internal interface IGarageHandler
    {
        int GarageCount { get; }
        IReadOnlyList<GarageSummary> GetGarages();
        GarageSummary GetGarageSummary(int garageId);
        IReadOnlyList<ParkedVehicleSummary> GetParkedVehicles(int garageId);
        GarageSummary AddGarage(string name, int capacity, VehicleType[] vehicleTypes);
        string AddVehicle(int garageId, VehicleType vehicleType);
        string PopulateGarage(int garageId);
        string EditVehicle(int garageId, int vehicleNumber);
        string RemoveVehicle(int garageId, int vehicleNumber);
    }
}
