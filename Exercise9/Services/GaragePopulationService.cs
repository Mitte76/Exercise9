using System;
using System.Collections.Generic;
using Exercise6.Facilities;
using Exercise6.Vehicles;

namespace Exercise6.Services
{
    internal static class GaragePopulationService
    {
        private const string LicensePlateLetters = "ABCDEFGHJKLMNOPRSTUWXYZ";
        private const string LicensePlateLastCharacters = LicensePlateLetters + "0123456789";

        private static readonly string[] CarBrands = ["Volvo", "Saab", "Toyota", "Honda", "BMW"];
        private static readonly string[] BusBrands = ["Volvo", "Scania", "Setra", "Iveco"];
        private static readonly string[] MotorcycleBrands =
        [
            "Honda",
            "Yamaha",
            "Suzuki",
            "Kawasaki",
        ];
        private static readonly string[] BoatBrands = ["Bayliner", "SeaRay", "Beneteau, Buster"];
        private static readonly string[] AirplaneBrands = ["Boeing", "Airbus", "Cessna", "Embraer"];
        private static readonly string[] Colors =
        [
            "White",
            "Black",
            "Red",
            "Blue",
            "Green",
            "Yellow",
            "Silver",
        ];
        private static readonly int[] BusSeatCounts = [10, 20, 30, 40, 50, 60];
        private static readonly int[] MotorcycleEngineVolumes = [125, 250, 600, 750, 1000];
        private static readonly decimal[] BoatLengths = [4.5m, 6.5m, 8m, 10m];
        private static readonly int[] AirplaneEngineCounts = [1, 2, 4];
        private static readonly FuelType[] FuelTypes = [FuelType.Diesel, FuelType.Gasoline];

        internal static string PopulateGarage(Garage garage, List<Garage> facilities)
        {
            if (garage.VehicleTypes.Length == 0)
                throw new InvalidOperationException(
                    "The garage does not support any vehicle types."
                );

            int availableSlots = garage.AvailableSlots;
            int vehiclesToAdd = availableSlots / 2 + availableSlots % 2;
            int addedVehicles = 0;

            for (int i = 0; i < vehiclesToAdd; i++)
            {
                VehicleType type = garage.VehicleTypes[
                    Random.Shared.Next(garage.VehicleTypes.Length)
                ];
                Vehicle vehicle = CreateVehicle(type, facilities);
                garage.ParkCar(vehicle);
                addedVehicles++;
            }

            return addedVehicles == 0
                ? $"{garage.Name} is already full."
                : $"Added {addedVehicles} vehicles to {garage.Name}.";
        }

        private static Vehicle CreateVehicle(VehicleType type, List<Garage> facilities)
        {
            string brand = GetRandomBrand(type);
            string color = PickRandom(Colors);

            return type switch
            {
                VehicleType.Airplane => new Airplane(
                    brand,
                    color,
                    PickRandom(AirplaneEngineCounts)
                ),
                VehicleType.Boat => new Boat(brand, color, PickRandom(BoatLengths)),
                VehicleType.Bus => new Bus(
                    brand,
                    color,
                    CreateUniqueLicensePlate(facilities),
                    PickRandom(BusSeatCounts)
                ),
                VehicleType.Car => new Car(
                    brand,
                    color,
                    CreateUniqueLicensePlate(facilities),
                    PickRandom(FuelTypes)
                ),
                VehicleType.Motorcycle => new Motorcycle(
                    brand,
                    color,
                    CreateUniqueLicensePlate(facilities),
                    PickRandom(MotorcycleEngineVolumes)
                ),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown vehicle type."
                ),
            };
        }

        private static string GetRandomBrand(VehicleType type)
        {
            string[] brands = type switch
            {
                VehicleType.Airplane => AirplaneBrands,
                VehicleType.Boat => BoatBrands,
                VehicleType.Bus => BusBrands,
                VehicleType.Car => CarBrands,
                VehicleType.Motorcycle => MotorcycleBrands,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown vehicle type."
                ),
            };

            return PickRandom(brands);
        }

        private static T PickRandom<T>(T[] options)
        {
            if (options.Length == 0)
                throw new InvalidOperationException("Vehicle option lists must not be empty.");

            return options[Random.Shared.Next(options.Length)];
        }

        private static string CreateUniqueLicensePlate(List<Garage> facilities)
        {
            while (true)
            {
                string letters = string.Concat(
                    LicensePlateLetters[Random.Shared.Next(LicensePlateLetters.Length)],
                    LicensePlateLetters[Random.Shared.Next(LicensePlateLetters.Length)],
                    LicensePlateLetters[Random.Shared.Next(LicensePlateLetters.Length)]
                );
                string digits = Random.Shared.Next(100).ToString("D2");
                char lastCharacter = LicensePlateLastCharacters[
                    Random.Shared.Next(LicensePlateLastCharacters.Length)
                ];
                string candidate = $"{letters}{digits}{lastCharacter}";

                if (!LicensePlateExists(candidate, facilities))
                    return candidate;
            }
        }

        private static bool LicensePlateExists(string licensePlate, List<Garage> facilities)
        {
            foreach (Garage facility in facilities)
            {
                foreach (ParkingSpot spot in facility.ParkingSpots)
                {
                    string? existingLicensePlate = spot.Vehicle switch
                    {
                        Car car => car.LicensePlateNumber,
                        Bus bus => bus.LicensePlateNumber,
                        Motorcycle motorcycle => motorcycle.LicensePlateNumber,
                        _ => null,
                    };

                    if (
                        string.Equals(
                            existingLicensePlate,
                            licensePlate,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                        return true;
                }
            }

            return false;
        }
    }
}
