namespace Exercise9.Vehicles
{
    public class Boat(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        decimal length
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public decimal Length { get; set; } = length;
        public override decimal ParkingSpaceRequirement => VehicleType.Boat.GetParkingSpaceRequirement();
    }
}
