using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Exercise9.Vehicles;

namespace Exercise9.Facilities
{
    internal abstract class Garage<T>(string name, int capacity)
        where T : Vehicle
    {
        public abstract VehicleType[] VehicleTypes { get; }

        public string Name { get; private set; } = name;
        public int Capacity { get; private set; } = capacity;
    }
}
