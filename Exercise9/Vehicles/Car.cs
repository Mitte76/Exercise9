namespace Exercise9.Vehicles
{
    internal class Car(
        string brand,
        string color,
        string licensePlateNumber,
        int numberOfWheels,
        FuelType fuelType
    ) : Vehicle(brand, color, licensePlateNumber, numberOfWheels)
    {
        public FuelType FuelType { get; set; } = fuelType;
    }

    public enum FuelType
    {
        Diesel,
        Gasoline,
    }
}
