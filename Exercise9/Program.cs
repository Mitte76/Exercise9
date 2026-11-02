using System.Collections.Generic;
using System.Threading.Tasks;
using Exercise9.Facilities;
using Exercise9.Services;
using Exercise9.Vehicles;
using Exercise9.UI;

namespace Exercise9
{
    internal class Program
    {
        static async Task Main()
        {
            GarageHandler garageHandler = new();
            MainMenu mainMenu = new();
            Garage<Vehicle>? selectedGarage;
            garageHandler.AddGarage();

            while ((selectedGarage = await mainMenu.ShowAsync(garageHandler)) != null)
                await GarageMenu.ShowAsync(selectedGarage);
        }
    }
}
