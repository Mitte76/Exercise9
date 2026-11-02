using Exercise9.Facilities;
using Exercise9.Services;
using Exercise9.UI;
using Exercise9.Vehicles;

namespace Exercise9.Tests;

public class GarageTests
{
    [Fact]
    public void TryParkVehicle_WhenGarageHasSpace_StoresVehicleAndSupportsForeach()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        Car car = CreateCar("ABC123");

        bool parked = garage.TryParkVehicle(car);

        Assert.True(parked);
        Assert.Same(car, Assert.Single(garage));
    }

    [Fact]
    public void TryParkVehicle_WhenGarageUsesConcreteVehicleType_StoresAndReturnsThatType()
    {
        Garage<Car> garage = new("Car garage", 1, [VehicleType.Car]);
        Car car = CreateCar("ABC123");

        bool parked = garage.TryParkVehicle(car);

        Assert.True(parked);
        Car storedCar = Assert.Single(garage);
        Assert.Same(car, storedCar);
        Assert.Same(car, garage.GetVehicleSlots()[0]);
    }

    [Fact]
    public void Vehicles_ExposeExpectedParkingSpaceRequirements()
    {
        Assert.Equal(3m, new Airplane("Boeing", "White", "ABH123", 6, 2).ParkingSpaceRequirement);
        Assert.Equal(2m, new Boat("Buster", "Blue", "BTH001", 0, 6m).ParkingSpaceRequirement);
        Assert.Equal(2m, new Bus("Volvo", "Red", "BUH123", 6, 40).ParkingSpaceRequirement);
        Assert.Equal(1m, CreateCar("ABC123").ParkingSpaceRequirement);
        Assert.Equal(1m / 3m, CreateMotorcycle("DEF456").ParkingSpaceRequirement);
    }

    [Fact]
    public void TryParkVehicle_WhenGarageHasOneSpace_AllowsThreeMotorcyclesAndRejectsFourth()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Motorcycle);
        Motorcycle first = CreateMotorcycle("ABC123");
        Motorcycle second = CreateMotorcycle("DEF456");
        Motorcycle third = CreateMotorcycle("GHK789");
        Motorcycle fourth = CreateMotorcycle("JKL789");

        Assert.True(garage.TryParkVehicle(first));
        Assert.True(garage.TryParkVehicle(second));
        Assert.True(garage.TryParkVehicle(third));
        Assert.False(garage.TryParkVehicle(fourth));
        Assert.Equal(0, garage.AvailableParkingSpaceUnits);
        Assert.Equal(3, garage.ParkingSpots[0].Vehicles.Count);
        Assert.Equal(3, garage.Count());
    }

    [Fact]
    public void GetParkedVehicles_WhenVehiclesUseDifferentSpaceSizes_ReportsPhysicalSpotNumbers()
    {
        GarageHandler handler = new();
        GarageSummary garage = handler.AddGarage(
            "Test garage",
            4,
            [VehicleType.Motorcycle, VehicleType.Car, VehicleType.Boat]
        );
        handler.AddVehicle(garage.Id, CreateMotorcycle("ABC123"));
        handler.AddVehicle(garage.Id, CreateMotorcycle("DEF456"));
        handler.AddVehicle(garage.Id, CreateMotorcycle("GHK789"));
        handler.AddVehicle(garage.Id, CreateCar("JKL789"));
        handler.AddVehicle(garage.Id, new Boat("Buster", "Blue", "BTH001", 0, 6m));

        IReadOnlyList<ParkedVehicleSummary> vehicles = handler.GetParkedVehicles(garage.Id);

        Assert.Equal([1], vehicles[0].ParkingSpotNumbers);
        Assert.Equal([1], vehicles[1].ParkingSpotNumbers);
        Assert.Equal([1], vehicles[2].ParkingSpotNumbers);
        Assert.Equal([2], vehicles[3].ParkingSpotNumbers);
        Assert.Equal([3, 4], vehicles[4].ParkingSpotNumbers);
    }

    [Fact]
    public void AddVehicle_WhenVehicleIsParked_ReportsItsParkingSpotNumbers()
    {
        GarageHandler handler = new();
        GarageSummary garage = handler.AddGarage("Test garage", 3, [VehicleType.Boat]);
        Boat boat = new("Buster", "Blue", "BTH001", 0, 6m);

        string result = handler.AddVehicle(garage.Id, boat);

        Assert.Equal("Boat parked in Test garage at parking spot(s) 1, 2.", result);
    }

    [Fact]
    public void TryParkVehicle_WhenVehicleNeedsMultipleSpaces_ConsumesThoseSpaces()
    {
        Garage<Vehicle> garage = CreateGarage(2, VehicleType.Boat, VehicleType.Car);
        Boat boat = new("Buster", "Blue", "BOA001", 0, 6m);

        bool parked = garage.TryParkVehicle(boat);

        Assert.True(parked);
        Assert.Equal(0, garage.AvailableParkingSpaceUnits);
        Assert.False(garage.TryParkVehicle(CreateCar("ABC123")));
    }

    [Fact]
    public void TryReplaceVehicle_WhenReplacementNeedsTooMuchSpace_KeepsExistingVehicle()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car, VehicleType.Boat);
        Car car = CreateCar("ABC123");
        garage.TryParkVehicle(car);

        bool replaced = garage.TryReplaceVehicle(
            1,
            new Boat("Buster", "Blue", "BOA001", 0, 6m)
        );

        Assert.False(replaced);
        Assert.Same(car, Assert.Single(garage));
        Assert.Equal(0, garage.AvailableParkingSpaceUnits);
    }

    [Fact]
    public void GetParkableVehicleTypes_WhenOnlyOneThirdSpaceRemains_ReturnsMotorcycleOnly()
    {
        GarageHandler handler = new();
        GarageSummary garage = handler.AddGarage(
            "Test garage",
            1,
            [VehicleType.Motorcycle, VehicleType.Car, VehicleType.Boat]
        );
        handler.AddVehicle(garage.Id, CreateMotorcycle("ABC123"));
        handler.AddVehicle(garage.Id, CreateMotorcycle("DEF456"));

        IReadOnlyList<VehicleType> parkableTypes = handler.GetParkableVehicleTypes(garage.Id);

        Assert.Equal([VehicleType.Motorcycle], parkableTypes);
    }

    [Fact]
    public void RestoreBackupSnapshot_WhenThreeMotorcyclesShareOneSpace_RestoresAllVehicles()
    {
        GarageHandler originalHandler = new();
        GarageSummary garage = originalHandler.AddGarage(
            "Test garage",
            1,
            [VehicleType.Motorcycle]
        );
        originalHandler.AddVehicle(garage.Id, CreateMotorcycle("ABC123"));
        originalHandler.AddVehicle(garage.Id, CreateMotorcycle("DEF456"));
        originalHandler.AddVehicle(garage.Id, CreateMotorcycle("GHK789"));
        IReadOnlyList<GarageSnapshot> snapshot = originalHandler.CreateBackupSnapshot();
        GarageHandler restoredHandler = new();

        restoredHandler.RestoreBackupSnapshot(snapshot);

        Assert.Equal(3, restoredHandler.GetParkedVehicles(1).Count);
        Assert.Equal(3, restoredHandler.GetGarageSummary(1).OccupiedParkingSpaceUnits);
    }

    [Fact]
    public void TryParkVehicle_WhenGarageIsFull_ReturnsFalseAndKeepsExistingVehicle()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        Car parkedCar = CreateCar("ABC123");
        garage.TryParkVehicle(parkedCar);
        Car secondCar = CreateCar("DEF456");

        bool parked = garage.TryParkVehicle(secondCar);

        Assert.False(parked);
        Assert.Same(parkedCar, Assert.Single(garage));
    }

    [Fact]
    public void TryParkVehicle_WhenRegistrationNumberAlreadyExists_ReturnsFalse()
    {
        Garage<Vehicle> garage = CreateGarage(2, VehicleType.Car);
        Car parkedCar = CreateCar("ABC123");
        garage.TryParkVehicle(parkedCar);
        Car duplicateCar = CreateCar("abc123");

        bool parked = garage.TryParkVehicle(duplicateCar);

        Assert.False(parked);
        Assert.Same(parkedCar, Assert.Single(garage));
    }

    [Fact]
    public void TryReplaceVehicle_WhenRegistrationNumberBelongsToAnotherVehicle_ReturnsFalse()
    {
        Garage<Vehicle> garage = CreateGarage(2, VehicleType.Car);
        Car firstCar = CreateCar("ABC123");
        Car secondCar = CreateCar("DEF456");
        garage.TryParkVehicle(firstCar);
        garage.TryParkVehicle(secondCar);

        bool replaced = garage.TryReplaceVehicle(2, CreateCar("abc123"));

        Assert.False(replaced);
        Assert.Equal(new[] { firstCar, secondCar }, garage);
    }

    [Fact]
    public void Garage_WhenCapacityExceedsMaximum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGarage(Garage<Vehicle>.MaximumCapacity + 1, VehicleType.Car)
        );
    }

    [Fact]
    public void Vehicle_WhenBrandAndColorContainSpacesOrHyphens_AcceptsValues()
    {
        Car car = new("Land Rover", "Dark-Blue", "ABC123", 4, FuelType.Gasoline);

        Assert.Equal("Land Rover", car.Brand);
        Assert.Equal("Dark-Blue", car.Color);
    }

    [Fact]
    public void VehicleForm_WhenBrandOrColorIsInvalid_PromptsAgain()
    {
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        using StringReader input = new(
            "!invalid!\nLand Rover\nDark@Blue\nDark Blue\nABC123\n4\n2\n"
        );
        using StringWriter output = new();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);

            Car car = (Car)VehicleForm.Create(VehicleType.Car);

            Assert.Equal("Land Rover", car.Brand);
            Assert.Equal("Dark Blue", car.Color);
            Assert.Contains("Invalid Brand", output.ToString());
            Assert.Contains("Invalid Color", output.ToString());
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void VehicleForm_WhenEditedRegistrationNumberAlreadyExists_PromptsAgain()
    {
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        GarageHandler handler = new();
        handler.AddGarage("Test garage", 2, [VehicleType.Car]);
        Car currentCar = CreateCar("ABC123");
        Car otherCar = CreateCar("DEF456");
        handler.AddVehicle(1, currentCar);
        handler.AddVehicle(1, otherCar);
        using StringReader input = new("\n\nDEF456\nGHK789\n\n\n");
        using StringWriter output = new();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);

            Car editedCar = (Car)
                VehicleForm.Create(
                    VehicleType.Car,
                    currentCar,
                    registrationNumber =>
                        handler.ValidateLicensePlateNumber(registrationNumber, currentCar)
                );

            Assert.Equal("GHK789", editedCar.LicensePlateNumber);
            Assert.Contains(
                "A vehicle with that registration number already exists.",
                output.ToString()
            );
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void TryParkVehicle_WhenVehicleTypeIsNotAllowed_ReturnsFalse()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        Boat boat = new("Buster", "Blue", "BOA001", 0, 6m);

        bool parked = garage.TryParkVehicle(boat);

        Assert.False(parked);
        Assert.Empty(garage);
    }

    [Fact]
    public void GetVehiclesWithPositions_WhenVehiclesAreParked_ReturnsTheirPositions()
    {
        Garage<Vehicle> garage = CreateGarage(3, VehicleType.Car);
        Car firstCar = CreateCar("ABC123");
        Car secondCar = CreateCar("DEF456");
        Car thirdCar = CreateCar("JKL789");
        garage.TryParkVehicle(firstCar);
        garage.TryParkVehicle(secondCar);
        garage.TryParkVehicle(thirdCar);
        garage.TryRemoveVehicle(2);

        (int Position, Vehicle Vehicle)[] parked = [.. garage.GetVehiclesWithPositions()];

        Assert.Collection(
            parked,
            first =>
            {
                Assert.Equal(1, first.Position);
                Assert.Same(firstCar, first.Vehicle);
            },
            third =>
            {
                Assert.Equal(3, third.Position);
                Assert.Same(thirdCar, third.Vehicle);
            }
        );
    }

    [Fact]
    public void TryGetVehicle_WhenPositionIsOccupied_ReturnsVehicle()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        Car car = CreateCar("ABC123");
        garage.TryParkVehicle(car);

        bool found = garage.TryGetVehicle(1, out Vehicle? vehicle);

        Assert.True(found);
        Assert.Same(car, vehicle);
    }

    [Fact]
    public void TryGetVehicle_WhenPositionIsInvalid_ReturnsFalse()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);

        bool found = garage.TryGetVehicle(0, out Vehicle? vehicle);

        Assert.False(found);
        Assert.Null(vehicle);
    }

    [Fact]
    public void TryRemoveVehicle_WhenPositionIsOccupied_RemovesVehicleAndFreesSpace()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        garage.TryParkVehicle(CreateCar("ABC123"));

        bool removed = garage.TryRemoveVehicle(1);
        bool parkedAgain = garage.TryParkVehicle(CreateCar("DEF456"));

        Assert.True(removed);
        Assert.True(parkedAgain);
    }

    [Fact]
    public void TryReplaceVehicle_WhenReplacementIsAllowed_ReplacesVehicleInPlace()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        garage.TryParkVehicle(CreateCar("ABC123"));
        Car replacement = CreateCar("DEF456");

        bool replaced = garage.TryReplaceVehicle(1, replacement);

        Assert.True(replaced);
        Assert.Same(replacement, Assert.Single(garage));
    }

    [Fact]
    public void TryReplaceVehicle_WhenReplacementTypeIsNotAllowed_KeepsExistingVehicle()
    {
        Garage<Vehicle> garage = CreateGarage(1, VehicleType.Car);
        Car existing = CreateCar("ABC123");
        garage.TryParkVehicle(existing);
        Boat replacement = new("Buster", "Blue", "BOA001", 0, 6m);

        bool replaced = garage.TryReplaceVehicle(1, replacement);

        Assert.False(replaced);
        Assert.Same(existing, Assert.Single(garage));
    }

    [Fact]
    public void SearchVehicles_WhenCriteriaAreCombined_MatchesAllFieldsAndAlternateValues()
    {
        GarageHandler handler = new();
        GarageSummary garage = handler.AddGarage("Test garage", 3, [VehicleType.Car]);
        handler.RestoreBackupSnapshot([
            new GarageSnapshot(
                "Test garage",
                3,
                [VehicleType.Car],
                [CreateCar("ABC123"), new Car("Toyota", "Red", "DEF456", 4, FuelType.Diesel), null]
            ),
        ]);
        Dictionary<string, IReadOnlyList<string>> criteria = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Brand"] = ["Volvo", "Toyota"],
            ["Color"] = ["black", "red"],
            ["NumberOfWheels"] = ["4"],
        };

        IReadOnlyList<ParkedVehicleSummary> matches = handler.SearchVehicles(
            garage.Id,
            new VehicleSearchCriteria(criteria)
        );

        Assert.Equal(2, matches.Count);
        Assert.Equal(new[] { "Volvo", "Toyota" }, matches.Select(vehicle => vehicle.Brand));
    }

    [Fact]
    public void SearchVehicles_WhenRegistrationNumberCaseDiffers_FindsVehicle()
    {
        GarageHandler handler = new();
        GarageSummary garage = handler.AddGarage("Test garage", 1, [VehicleType.Car]);
        handler.RestoreBackupSnapshot([
            new GarageSnapshot("Test garage", 1, [VehicleType.Car], [CreateCar("ABC123")]),
        ]);
        Dictionary<string, IReadOnlyList<string>> criteria = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LicensePlateNumber"] = ["abc 123"],
        };

        IReadOnlyList<ParkedVehicleSummary> matches = handler.SearchVehicles(
            garage.Id,
            new VehicleSearchCriteria(criteria)
        );

        Assert.Single(matches);
        Assert.Equal("Volvo", matches[0].Brand);
    }

    private static Garage<Vehicle> CreateGarage(int capacity, params VehicleType[] allowedTypes)
    {
        return new Garage<Vehicle>("Test garage", capacity, allowedTypes);
    }

    private static Car CreateCar(string registrationNumber)
    {
        return new Car("Volvo", "Black", registrationNumber, 4, FuelType.Gasoline);
    }

    private static Motorcycle CreateMotorcycle(string registrationNumber)
    {
        return new Motorcycle("Honda", "Purple", registrationNumber, 2, 600);
    }
}
