using System;
using System.Collections.Generic;
using Exercise6.Facilities;

namespace Exercise6.Vehicles
{
    internal class Motorcycle(
        string brand,
        string color,
        string licensePlateNumber,
        int cylinderVolume
    ) : Vehicle(brand, color)
    {
        public string LicensePlateNumber
        {
            get;
            set => field = ValidateLicensePlateNumber(value);
        } = ValidateLicensePlateNumber(licensePlateNumber);

        public int CylinderVolume { get; set; } = cylinderVolume;

        public static Motorcycle Create(List<Garage> facilities)
        {
            var (brand, color) = GetCommonProperties("Motorcycle");

            string? licensePlate = ReadInput("Enter license plate (e.g. ABC123 or ABC12D):");
            if (licensePlate == null)
                throw new OperationCanceledException("Adding Motorcycle aborted");

            licensePlate = ValidateLicensePlateNumber(licensePlate, facilities);

            string? cylinderVolumeInput = ReadInput("Enter engine volume (cc):");
            if (cylinderVolumeInput == null)
                throw new OperationCanceledException("Adding Motorcycle aborted");

            if (!int.TryParse(cylinderVolumeInput, out int cylinderVolume) || cylinderVolume <= 0)
                throw new ArgumentException("Engine volume must be a positive number.");

            return new Motorcycle(brand, color, licensePlate, cylinderVolume);
        }
    }
}
