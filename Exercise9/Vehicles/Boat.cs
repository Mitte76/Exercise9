using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise6.Vehicles
{
    internal class Boat(string brand, string color, decimal length) : Vehicle(brand, color)
    {
        public decimal Length { get; set; } = length;

        public static Boat Create()
        {
            var (brand, color) = GetCommonProperties("Boat");

            string? lengthInput = ReadInput("Enter length of boat:");
            if (lengthInput == null)
                throw new OperationCanceledException("Adding Boat aborted");

            if (!decimal.TryParse(lengthInput, out decimal length))
                throw new ArgumentException("Invalid length");

            return new Boat(brand, color, length);
        }
    }
}
