using System;
using System.Collections.Generic;
using Exercise6.Vehicles;

namespace Exercise6.Facilities
{
    internal class Hangar(string name, int capacity) : Garage(name, capacity)
    {
        public override VehicleType[] VehicleTypes => [VehicleType.Airplane];
    }
}
