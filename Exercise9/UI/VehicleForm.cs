using System;
using Exercise9.Vehicles;

namespace Exercise9.UI
{
    internal static class VehicleForm
    {
        internal static Vehicle Create(
            VehicleType type,
            Vehicle? currentVehicle = null,
            Func<string, string>? validateRegistrationNumber = null
        )
        {
            string brand = ReadString("Brand", currentVehicle?.Brand, Vehicle.ValidateBrand);
            string color = ReadString("Color", currentVehicle?.Color, Vehicle.ValidateColor);
            string registrationNumber = ReadString(
                "Registration number (e.g. ABC123 or ABC12D)",
                currentVehicle?.LicensePlateNumber,
                validateRegistrationNumber ?? (value => Vehicle.ValidateLicensePlateNumber(value))
            );
            int numberOfWheels = ReadInteger("Number of wheels", currentVehicle?.NumberOfWheels, 0);

            return type switch
            {
                VehicleType.Airplane => new Airplane(
                    brand,
                    color,
                    registrationNumber,
                    numberOfWheels,
                    ReadInteger(
                        "Number of engines",
                        (currentVehicle as Airplane)?.NumberOfEngines,
                        1
                    )
                ),
                VehicleType.Boat => new Boat(
                    brand,
                    color,
                    registrationNumber,
                    numberOfWheels,
                    ReadDecimal("Boat length", (currentVehicle as Boat)?.Length, 0.01m)
                ),
                VehicleType.Bus => new Bus(
                    brand,
                    color,
                    registrationNumber,
                    numberOfWheels,
                    ReadInteger("Number of seats", (currentVehicle as Bus)?.NumberOfSeats, 1)
                ),
                VehicleType.Car => new Car(
                    brand,
                    color,
                    registrationNumber,
                    numberOfWheels,
                    ReadFuelType((currentVehicle as Car)?.FuelType)
                ),
                VehicleType.Motorcycle => new Motorcycle(
                    brand,
                    color,
                    registrationNumber,
                    numberOfWheels,
                    ReadInteger(
                        "Engine volume (cc)",
                        (currentVehicle as Motorcycle)?.CylinderVolume,
                        1
                    )
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        private static string ReadString(
            string label,
            string? currentValue,
            Func<string, string>? validate = null
        )
        {
            while (true)
            {
                Console.Write(
                    currentValue == null
                        ? $"Enter {label} (empty to cancel): "
                        : $"Enter {label} [{currentValue}] (empty to keep): "
                );
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    if (currentValue != null)
                        return currentValue;
                    throw new OperationCanceledException("Vehicle creation cancelled.");
                }

                string value = input.Trim();
                if (validate == null)
                    return value;

                try
                {
                    return validate(value);
                }
                catch (ArgumentException exception)
                {
                    Console.WriteLine($"{exception.Message}. Please try again.");
                }
            }
        }

        private static int ReadInteger(string label, int? currentValue, int minimum)
        {
            while (true)
            {
                Console.Write(
                    currentValue == null
                        ? $"Enter {label} (minimum {minimum}, empty to cancel): "
                        : $"Enter {label} [{currentValue}] (minimum {minimum}, empty to keep): "
                );
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    if (currentValue.HasValue)
                        return currentValue.Value;
                    throw new OperationCanceledException("Vehicle creation cancelled.");
                }

                if (int.TryParse(input, out int value) && value >= minimum)
                    return value;
                Console.WriteLine($"Enter a whole number greater than or equal to {minimum}.");
            }
        }

        private static decimal ReadDecimal(string label, decimal? currentValue, decimal minimum)
        {
            while (true)
            {
                Console.Write(
                    currentValue == null
                        ? $"Enter {label} (minimum {minimum}, empty to cancel): "
                        : $"Enter {label} [{currentValue}] (minimum {minimum}, empty to keep): "
                );
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    if (currentValue.HasValue)
                        return currentValue.Value;
                    throw new OperationCanceledException("Vehicle creation cancelled.");
                }

                if (decimal.TryParse(input, out decimal value) && value >= minimum)
                    return value;
                Console.WriteLine($"Enter a number greater than or equal to {minimum}.");
            }
        }

        private static FuelType ReadFuelType(FuelType? currentValue)
        {
            while (true)
            {
                Console.Write(
                    currentValue == null
                        ? "Select fuel type (1. Diesel, 2. Gasoline; empty to cancel): "
                        : $"Select fuel type (1. Diesel, 2. Gasoline) [{currentValue}] (empty to keep): "
                );
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    if (currentValue.HasValue)
                        return currentValue.Value;
                    throw new OperationCanceledException("Vehicle creation cancelled.");
                }

                if (input.Trim() == "1")
                    return FuelType.Diesel;
                if (input.Trim() == "2")
                    return FuelType.Gasoline;
                Console.WriteLine("Select 1 for Diesel or 2 for Gasoline.");
            }
        }
    }
}
