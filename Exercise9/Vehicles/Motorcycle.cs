namespace Exercise9.Vehicles
{
    public class Motorcycle(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        int cylinderVolume
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public int CylinderVolume { get; set; } = cylinderVolume;
        public override decimal ParkingSpaceRequirement => VehicleType.Motorcycle.GetParkingSpaceRequirement();
    }
}
