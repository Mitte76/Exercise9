namespace Exercise9.Vehicles
{
    public class Airplane(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        int numberOfEngines
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public int NumberOfEngines { get; set; } = numberOfEngines;
        public override decimal ParkingSpaceRequirement => VehicleType.Airplane.GetParkingSpaceRequirement();
    }
}
