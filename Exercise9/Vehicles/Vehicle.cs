using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Exercise6.Facilities;

namespace Exercise6.Vehicles
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
            List<Garage>? facilities = null,
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
                foreach (Garage facility in facilities)
                {
                    foreach (ParkingSpot spot in facility.ParkingSpots)
                    {
                        if (
                            spot.Vehicle is null
                            || ReferenceEquals(spot.Vehicle, vehicleBeingUpdated)
                        )
                            continue;

                        string? existing = spot.Vehicle switch
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

        //public static string ValidateLicensePlateNumber(string registratíonNumber)
        //{
        //    string pattern = @"^[A-HJ-PR-UW-Z]{3}\s?[0-9]{2}[A-HJ-PR-UW-Z0-9]$";
        //    Regex regex = new(pattern, RegexOptions.IgnoreCase);

        //    if (string.IsNullOrEmpty(registratíonNumber) || !regex.IsMatch(registratíonNumber))
        //        throw new ArgumentException("Licence plate number is not valid");

        //    return registratíonNumber.ToUpper();
        //}

        //internal static string ValidateLicensePlateNumber(
        //    string registratíonNumber,
        //    List<Garage> facilities
        //)
        //{
        //    string validatedNumber = ValidateLicensePlateNumber(registratíonNumber);

        //    foreach (Garage facility in facilities)
        //    {
        //        foreach (ParkingSpot parkingSpot in facility.ParkingSpots)
        //        {
        //            string? existingNumber = parkingSpot.Vehicle switch
        //            {
        //                Car car => car.LicensePlateNumber,
        //                Bus bus => bus.LicensePlateNumber,
        //                Motorcycle motorcycle => motorcycle.LicensePlateNumber,
        //                _ => null,
        //            };

        //            if (
        //                existingNumber?.Equals(validatedNumber, StringComparison.OrdinalIgnoreCase)
        //                == true
        //            )
        //            {
        //                throw new ArgumentException(
        //                    "A vehicle with that license plate already exists"
        //                );
        //            }
        //        }
        //    }

        //    return validatedNumber;
        //}
    }
}
