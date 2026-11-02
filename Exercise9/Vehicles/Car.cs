using System;
using System.Collections.Generic;
using Exercise6.Facilities;

namespace Exercise6.Vehicles
{
    [Serializable()]
    internal class Car(string brand, string color, string licensePlateNumber, FuelType fuelType)
        : Vehicle(brand, color)
    {
        public FuelType FuelType { get; set; } = fuelType;

        public string LicensePlateNumber
        {
            get;
            set => field = ValidateLicensePlateNumber(value);
        } = ValidateLicensePlateNumber(licensePlateNumber);

        public static Car Create(List<Garage> facilities)
        {
            var (brand, color) = GetCommonProperties("Car");

            string? licensePlate = ReadInput("Enter license plate (e.g. ABC123 or ABC12D):");
            if (licensePlate == null)
                throw new OperationCanceledException("Adding Car Aborted");

            licensePlate = ValidateLicensePlateNumber(licensePlate, facilities);

            FuelType fuelType = ReadInput(
                "Select fuel type:\n1. Diesel\n2. Gasoline\n",
                true
            ) switch
            {
                "1" => FuelType.Diesel,
                "2" => FuelType.Gasoline,
                _ => throw new ArgumentException("Invalid fuel type"),
            };

            return new Car(brand, color, licensePlate, fuelType);
        }
    }

    public enum FuelType
    {
        Diesel,
        Gasoline,
    }
}
