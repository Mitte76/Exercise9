using System.Threading.Tasks;
using Exercise9.Services;
using Exercise9.UI;

namespace Exercise9
{
    internal class Program
    {
        static async Task Main()
        {
            IGarageHandler garageHandler = new GarageHandler();
            MainMenu mainMenu = new();
            GarageSummary? selectedGarage;

            while ((selectedGarage = await mainMenu.Show(garageHandler)) != null)
                await GarageMenu.Show(selectedGarage.Id, garageHandler);
        }
    }
}
