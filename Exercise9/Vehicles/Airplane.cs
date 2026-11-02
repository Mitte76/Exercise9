using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise6.Vehicles
{
    internal class Airplane(string brand, string color, int numberOfEngines) : Vehicle(brand, color)
    {
        public int NumberOfEngines { get; set; } = numberOfEngines;

        public static Airplane Create()
        {
            var (brand, color) = GetCommonProperties("Airplane");

            string? amountIn = ReadInput("Enter number of engines:");
            if (amountIn == null)
                throw new OperationCanceledException("Adding Airplane aborted");

            if (!int.TryParse(amountIn, out int amount))
                throw new ArgumentException("Invalid amount of engines");

            return new Airplane(brand, color, amount);
        }
    }
}
