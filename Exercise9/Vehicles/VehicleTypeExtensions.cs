namespace Exercise9.Vehicles
{
    public static class VehicleTypeExtensions
    {
        public static decimal GetParkingSpaceRequirement(this VehicleType vehicleType)
        {
            return vehicleType switch
            {
                VehicleType.Airplane => 3m,
                VehicleType.Boat => 2m,
                VehicleType.Bus => 2m,
                VehicleType.Car => 1m,
                VehicleType.Motorcycle => 1m / 3m,
                _ => throw new System.ArgumentOutOfRangeException(nameof(vehicleType)),
            };
        }
    }
}
