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
    public class Vehicle(string brand, string color)
    {
        public string Color { get; set; } = IsValidInput(color, "Color");
        public string Brand
        {
            get;
            set => field = IsValidInput(value, "Brand");
        } = IsValidInput(brand, "Brand");

        private static string IsValidInput(string color, string property)
        {
            if (!Regex.IsMatch(color, @"^[\p{L}]+$"))
                throw new ArgumentException($"Invalid {property}");
            return color;
        }

        protected static (string Brand, string Color) GetCommonProperties(string type)
        {
            string? brand = ReadInput("Enter brand:");
            if (brand == null)
                throw new OperationCanceledException($"Adding {type} aborted");

            string? color = ReadInput("Enter color:");
            if (color == null)
                throw new OperationCanceledException($"Adding {type} aborted");

            return (brand, color);
        }

        protected static string? ReadInput(string prompt, bool skip = false)
        {
            Console.Write($"{prompt}");
            if (!skip)
                Console.Write(" Leave empty to exit\n");

            string? input = Console.ReadLine();

            return string.IsNullOrWhiteSpace(input) ? null : input;
        }

        internal static string ValidateLicensePlateNumber(
            string registrationNumber,
            List<Garage<Vehicle>>? facilities = null,
            Vehicle? vehicleBeingUpdated = null
        )
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                throw new ArgumentException("License plate cannot be empty.");

            string normalized = registrationNumber.Trim().Replace(" ", "").ToUpperInvariant();

            if (!Regex.IsMatch(normalized, @"^[A-HJ-PR-UW-Z]{3}[0-9]{2}[A-HJ-PR-UW-Z0-9]$"))
                throw new ArgumentException("Licence plate number is not valid");

            if (facilities != null)
            {
                foreach (Garage<Vehicle> facility in facilities)
                {
                    foreach (Vehicle parkedVehicle in facility)
                    {
                        if (ReferenceEquals(parkedVehicle, vehicleBeingUpdated))
                            continue;

                        string? existing = parkedVehicle switch
                        {
                            Car car => car.LicensePlateNumber,
                            Bus bus => bus.LicensePlateNumber,
                            Motorcycle motorcycle => motorcycle.LicensePlateNumber,
                            _ => null,
                        };

                        if (string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase))
                            throw new ArgumentException(
                                "A vehicle with that license plate already exists"
                            );
                    }
                }
            }

            return normalized;
        }

    }
}
