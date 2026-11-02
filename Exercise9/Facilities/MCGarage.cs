using System;
using System.Collections.Generic;
using Exercise6.Vehicles;

namespace Exercise6.Facilities
{
    internal class MCGarage(string name, int capacity) : Garage(name, capacity)
    {
        public override VehicleType[] VehicleTypes => [VehicleType.Motorcycle];
    }
}
