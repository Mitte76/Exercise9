using System.Collections.Generic;
using Exercise9.Vehicles;

namespace Exercise9.Services
{
    internal sealed record GarageSnapshot(
        string Name,
        int Capacity,
        VehicleType[] VehicleTypes,
        Vehicle?[] Vehicles
    );

    internal interface IGarageHandler
    {
        int GarageCount { get; }
        IReadOnlyList<GarageSummary> GetGarages();
        IReadOnlyList<GarageSnapshot> CreateBackupSnapshot();
        void RestoreBackupSnapshot(IReadOnlyList<GarageSnapshot> snapshots);
        GarageSummary GetGarageSummary(int garageId);
        IReadOnlyList<ParkedVehicleSummary> GetParkedVehicles(int garageId);
        IReadOnlyList<VehicleType> GetParkableVehicleTypes(int garageId);
        IReadOnlyList<ParkedVehicleSummary> SearchVehicles(
            int garageId,
            VehicleSearchCriteria criteria
        );
        GarageSummary AddGarage(string name, int capacity, VehicleType[] vehicleTypes);
        Vehicle? GetVehicle(int garageId, int vehicleNumber);
        string AddVehicle(int garageId, Vehicle vehicle);
        string PopulateGarage(int garageId);
        string ValidateLicensePlateNumber(string registrationNumber, Vehicle? vehicleBeingUpdated = null);
        string EditVehicle(int garageId, int vehicleNumber, Vehicle replacement);
        string RemoveVehicle(int garageId, int vehicleNumber);
    }
}
