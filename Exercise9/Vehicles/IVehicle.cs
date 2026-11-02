namespace Exercise9.Vehicles
{
    public interface IVehicle
    {
        string Brand { get; set; }
        string Color { get; set; }
        string LicensePlateNumber { get; set; }
        int NumberOfWheels { get; set; }
    }
}
