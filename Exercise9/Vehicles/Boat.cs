namespace Exercise9.Vehicles
{
    internal class Boat(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        decimal length
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public decimal Length { get; set; } = length;
    }
}
