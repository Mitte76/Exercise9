namespace Exercise9.Vehicles
{
    internal class Bus(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        int numberOfSeats
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public int NumberOfSeats { get; set; } = numberOfSeats;
    }
}
