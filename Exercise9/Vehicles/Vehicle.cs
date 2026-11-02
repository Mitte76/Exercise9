using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Exercise9.Facilities;

namespace Exercise9.Vehicles
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Airplane), "Airplane")]
    [JsonDerivedType(typeof(Boat), "Boat")]
    [JsonDerivedType(typeof(Bus), "Bus")]
    [JsonDerivedType(typeof(Car), "Car")]
    [JsonDerivedType(typeof(Motorcycle), "Motorcycle")]
    public class Vehicle(string brand, string color, string licensePlateNumber, int numberOfWheels)
        : IVehicle
    {
        public string Color
        {
            get;
            set => field = ValidateColor(value);
        } = ValidateColor(color);

        public string Brand
        {
            get;
            set => field = ValidateBrand(value);
        } = ValidateBrand(brand);

        public string LicensePlateNumber
        {
            get;
            set => field = ValidateLicensePlateNumber(value);
        } = ValidateLicensePlateNumber(licensePlateNumber);

        public int NumberOfWheels
        {
            get;
            set =>
                field = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        } =
            numberOfWheels >= 0
                ? numberOfWheels
                : throw new ArgumentOutOfRangeException(nameof(numberOfWheels));

        public virtual decimal ParkingSpaceRequirement => 1m;

        internal static string ValidateBrand(string value) => ValidateDescriptiveInput(value, "Brand");

        internal static string ValidateColor(string value) => ValidateDescriptiveInput(value, "Color");

        private static string ValidateDescriptiveInput(string value, string property)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Invalid {property}");

            string normalized = value.Trim();
            if (!Regex.IsMatch(normalized, @"^[\p{L}\p{N}]+(?:[ \-][\p{L}\p{N}]+)*$"))
                throw new ArgumentException($"Invalid {property}");

            return normalized;
        }

        internal static string ValidateLicensePlateNumber(
            string registrationNumber,
            List<Garage<Vehicle>>? facilities = null,
            Vehicle? vehicleBeingUpdated = null
        )
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                throw new ArgumentException("Registration number cannot be empty.");

            string normalized = registrationNumber.Trim().Replace(" ", "").ToUpperInvariant();

            if (!Regex.IsMatch(normalized, @"^[A-HJ-PR-UW-Z]{3}[0-9]{2}[A-HJ-PR-UW-Z0-9]$"))
                throw new ArgumentException("Registration number is not valid.");

            if (facilities != null)
            {
                foreach (Garage<Vehicle> facility in facilities)
                {
                    foreach (Vehicle parkedVehicle in facility)
                    {
                        if (ReferenceEquals(parkedVehicle, vehicleBeingUpdated))
                            continue;

                        if (
                            string.Equals(
                                parkedVehicle.LicensePlateNumber,
                                normalized,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                            throw new ArgumentException(
                                "A vehicle with that registration number already exists."
                            );
                    }
                }
            }

            return normalized;
        }
    }
}
