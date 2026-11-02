using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exercise9.Facilities;
using Exercise9.Vehicles;

namespace Exercise9.Services
{
    internal class GarageHandler()
    {
        private readonly List<Garage<Vehicle>> _garages = [];

        public List<string> GetGarageSummaries()
        {
            return _garages.Select(garage => $"{garage.Name} – {garage.Capacity} platser").ToList();
        }
    }
}
