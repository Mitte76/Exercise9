using System.Collections.Generic;
using System.Threading.Tasks;
using Exercise9.Services;
using Exercise9.UI;

namespace Exercise9
{
    internal class Program
    {
        static async Task Main()
        {
            GarageHandler garageHandler = new();
            MainMenu mainMenu = new(garageHandler: garageHandler);

            await mainMenu.Show().ShowMenu();
        }
    }
}
